ASSEMBLY_NAME := $(shell cat build.csproj | tr -d '[:space:]' | sed 's/.*AssemblyName>\([^<]\+\).*/\1/')
MOD_VERSION   := $(shell (git describe --tags 2> /dev/null || echo 'v0.0.0') | cut -c2-)
MOD_ID        := $(shell echo "$(ASSEMBLY_NAME)" | tr '[:upper:]' '[:lower:]')

BUILD_ROOT   := ${PWD}/$(shell cat Directory.Build.props | tr -d '[:space:]' | sed 's/.*BuildRoot>\([^<]\+\).*/\1/')
INTER_ROOT   := $(BUILD_ROOT)/obj
OUTPUT_ROOT  := $(BUILD_ROOT)/out
STAGING_ROOT := $(BUILD_ROOT)/staging
RELEASE_ROOT := $(BUILD_ROOT)/release

FALLBACK_JQ  := .tools/jq

ifeq ($(shell command -v jq 2> /dev/null || echo $(FALLBACK_JQ)),$(FALLBACK_JQ))
  JQ := $(FALLBACK_JQ)
else
  JQ := jq
  FALLBACK_JQ :=
endif

ZIP_NAME_BASE := $(shell echo $(ASSEMBLY_NAME) | sed 's/\([A-Z]\)/-\L\1/g;s/^-//')

CSHARP_FILES := $(shell find src -type f -name '*.cs')
BUNDLED_FILES := modinfo.json
INCLUDED_FILES := $(STAGING_ROOT)/modicon.png $(STAGING_ROOT)/modinfo.json

#
# Meta Targets
#

.PHONY: default
default:
	@echo
	@echo "Assembly Name  = $(ASSEMBLY_NAME)"
	@echo "Mod Version    = $(MOD_VERSION)"
	@echo "Mod ID         = $(MOD_ID)"
	@echo
	@echo "JQ Executable  = $(JQ)"
	@echo
	@echo "Build Root     = $(BUILD_ROOT)"
	@echo "Output Root    = $(OUTPUT_ROOT)"
	@echo "Staging Root   = $(STAGING_ROOT)"
	@echo "Release Root   = $(RELEASE_ROOT)"
	@echo
	@echo "Zip Prefix     = $(ZIP_NAME_BASE)"
	@echo
	@echo "Debug Bundle   = $(DEBUG_ZIP_TARGET)"
	@echo "Release Bundle = $(RELEASE_ZIP_TARGET)"
	@echo
	@echo "C# Files       = $(shell echo $(CSHARP_FILES) | wc -w)"
	@echo
	@echo "Debug Outputs  = $(DEBUG_OUTPUT_FILES)"
	@echo

#
# Debug Builds
#

PROFILE_DEBUG      := Debug
DEBUG_OUT_DIR      := $(OUTPUT_ROOT)/$(PROFILE_DEBUG)
DEBUG_ZIP_TARGET   := $(RELEASE_ROOT)/$(ZIP_NAME_BASE)-$(MOD_VERSION)-DEBUG.zip
DEBUG_OUTPUT_FILES := $(foreach file,$(ASSEMBLY_NAME).dll $(ASSEMBLY_NAME).pdb,$(DEBUG_OUT_DIR)/$(file))

.PHONY: debug-build
debug-build: $(DEBUG_OUTPUT_FILES)

.PHONY: debug-release
debug-release: $(DEBUG_ZIP_TARGET)

$(DEBUG_ZIP_TARGET): $(DEBUG_OUTPUT_FILES) $(INCLUDED_FILES)
	@rm -f $(DEBUG_ZIP_TARGET)
	@mkdir -p $(RELEASE_ROOT)
	@cp -t $(STAGING_ROOT) $(DEBUG_OUTPUT_FILES)
	@cd $(STAGING_ROOT) && zip -rq $(DEBUG_ZIP_TARGET) *

$(DEBUG_OUTPUT_FILES): $(CSHARP_FILES)
	@rm -rf $(DEBUG_OUT_DIR) $(INTER_ROOT)
	@dotnet build build.csproj /p:Configuration=$(PROFILE_DEBUG)

#
# Release Builds
#

PROFILE_RELEASE      := Release
RELEASE_OUT_DIR      := $(OUTPUT_ROOT)/$(PROFILE_RELEASE)
RELEASE_ZIP_TARGET   := $(RELEASE_ROOT)/$(ZIP_NAME_BASE)-$(MOD_VERSION).zip
RELEASE_OUTPUT_FILES := $(RELEASE_OUT_DIR)/$(ASSEMBLY_NAME).dll

.PHONY: release-build
prod-build: clean $(RELEASE_OUTPUT_FILES)

.PHONY: release-release
prod-release: clean $(RELEASE_ZIP_TARGET)

$(RELEASE_ZIP_TARGET): $(RELEASE_OUTPUT_FILES) $(INCLUDED_FILES)
	@rm -f $(RELEASE_ZIP_TARGET)
	@mkdir -p $(RELEASE_ROOT)
	@cp -t $(STAGING_ROOT) $(RELEASE_OUTPUT_FILES)
	@cd $(STAGING_ROOT) && zip -rq $(RELEASE_ZIP_TARGET) *

$(RELEASE_OUTPUT_FILES): $(CSHARP_FILES)
	@rm -rf $(RELEASE_OUT_DIR) $(INTER_ROOT)
	@dotnet build build.csproj /p:Configuration=$(PROFILE_RELEASE)

#
#  Utility Targets
#

.PHONY: check-env
check-env:
	@[ ! -z "$${VINTAGE_STORY}" ] \
		|| (echo; echo "You might want to set \$$VINTAGE_STORY or you're gonna have a bad time."; echo; false)

.PHONY: clean
clean:
	@rm -rf $(OUTPUT_ROOT) $(STAGING_ROOT) $(INT)

#
# Playtesting Setup
#

TESTING_DATA_PATH := ${PWD}/testing
TEST_MOD_ZIP_NAME := test-target.zip

.PHONY: playtest
playtest: check-env $(TESTING_DATA_PATH)/clientsettings.json $(DEBUG_ZIP_TARGET)
	@mkdir -p $(TESTING_DATA_PATH)/Mods
	@cp $(DEBUG_ZIP_TARGET) $(TESTING_DATA_PATH)/Mods/$(TEST_MOD_ZIP_NAME)
	@$${VINTAGE_STORY}/Vintagestory --dataPath=$(TESTING_DATA_PATH)

prod-playtest: check-env $(TESTING_DATA_PATH)/clientsettings.json $(RELEASE_ZIP_TARGET)
	@mkdir -p $(TESTING_DATA_PATH)/Mods
	@cp $(RELEASE_ZIP_TARGET) $(TESTING_DATA_PATH)/Mods/$(TEST_MOD_ZIP_NAME)
	@$${VINTAGE_STORY}/Vintagestory --dataPath=$(TESTING_DATA_PATH)


$(TESTING_DATA_PATH)/clientsettings.json: $(FALLBACK_JQ)
	@mkdir -p $(TESTING_DATA_PATH)
	@if [ -f "$${HOME}/.config/VintagestoryData/clientsettings.json" ]; then \
	  $(JQ) ".stringListSettings.disabledMods = [] \
	  	| .stringListSettings.modPaths = [\"Mods\", \"$(TESTING_DATA_PATH)/Mods\"] \
	  	| .boolSettings.developerMode = true \
	  	| .boolSettings.extendedDebugInfo = true \
	  	| .boolSettings.startupErrorDialog = true" \
	    "$${HOME}/.config/VintagestoryData/clientsettings.json" > $@; \
	else \
	  echo "Don't know where your client settings are, you're gonna have to start from a clean slate."; \
	fi

#
# File Targets
#

$(FALLBACK_JQ):
	@mkdir -p $(dir $@)
	@wget -q https://github.com/jqlang/jq/releases/download/jq-1.7.1/jq-linux-amd64 -O $@
	@chmod +x $@

$(STAGING_ROOT)/modicon.png: $(STAGING_ROOT)
	@cp assets/icons/icon-128.png "$@"

$(STAGING_ROOT)/modinfo.json: assets/templates/modinfo.base.json $(STAGING_ROOT) $(FALLBACK_JQ)
	@$(JQ) '.modid = "$(MOD_ID)" | .version = "$(MOD_VERSION)" | .name = "$(ASSEMBLY_NAME)"' $< > $@

$(RELEASE_ROOT):
	@mkdir -p "$@"

$(STAGING_ROOT):
	@mkdir -p "$@"
