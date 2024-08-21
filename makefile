ASSEMBLY_NAME := $(shell cat build.csproj | tr -d '[:space:]' | sed 's/.*AssemblyName>\([^<]\+\).*/\1/')
MOD_VERSION   := $(shell git name-rev --tags --name-only $$(git rev-parse HEAD))
MOD_ID        := $(shell echo "$(ASSEMBLY_NAME)" | tr '[:upper:]' '[:lower:]')

BUILD_ROOT   := ${PWD}/$(shell cat Directory.Build.props | tr -d '[:space:]' | sed 's/.*BuildRoot>\([^<]\+\).*/\1/')
INTER_ROOT   := $(BUILD_ROOT)/obj
OUTPUT_ROOT  := $(BUILD_ROOT)/out
STAGING_ROOT := $(BUILD_ROOT)/staging
RELEASE_ROOT := $(BUILD_ROOT)/release

JQ_DEP := $(shell command -v jq &> /dev/null || echo ".tools/jq")
ifeq ($(JQ_DEP), ".tools/jq")
  JQ := $(JQ_DEP)
else
  JQ := jq
endif

ZIP_NAME_PREFIX := $(shell echo $(ASSEMBLY_NAME) | sed 's/\([A-Z]\)/-\L\1/g;s/^-//')

TESTING_DATA_PATH := ${PWD}/testing

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
	@echo "Build Root     = $(BUILD_ROOT)"
	@echo "Output Root    = $(OUTPUT_ROOT)"
	@echo "Staging Root   = $(STAGING_ROOT)"
	@echo "Release Root   = $(RELEASE_ROOT)"
	@echo
	@echo "Zip Prefix     = $(ZIP_NAME_PREFIX)"
	@echo
	@echo "Debug Bundle   = $(DEBUG_ZIP_TARGET)"
	@echo "Release Bundle = $(RELEASE_ZIP_TARGET)"
	@echo
	@echo "C# Files       = $(CSHARP_FILES)"
	@echo
	@echo "Debug Outputs  = $(DEBUG_OUTPUT_FILES)"
	@echo

.PHONY: clean-playtest
clean-playtest: check-env
	@rm -rf $(TESTING_DATA_PATH)
	@mkdir -p $(TESTING_DATA_PATH)/Mods
	@cp $(DEBUG_ZIP_TARGET) $(TESTING_DATA_PATH)/Mods
#	@utils/setup-client.sh || echo 'sorry for the inconvenience'
	@$${VINTAGE_STORY}/Vintagestory --dataPath=$(TESTING_DATA_PATH)

.PHONY: playtest
playtest: check-env
	@cp $(DEBUG_ZIP_TARGET) $(TESTING_DATA_PATH)/Mods
	@$${VINTAGE_STORY}/Vintagestory --dataPath=$(TESTING_DATA_PATH)

#
# Debug Builds
#

PROFILE_DEBUG      := Debug
DEBUG_OUT_DIR      := $(OUTPUT_ROOT)/$(PROFILE_DEBUG)
DEBUG_ZIP_TARGET   := $(RELEASE_ROOT)/$(ZIP_NAME_PREFIX)-$(MOD_VERSION)-DEBUG.zip
DEBUG_OUTPUT_FILES := $(foreach file, $(ASSEMBLY_NAME).dll $(ASSEMBLY_NAME).pdb, $(DEBUG_OUT_DIR)/$(file))

.PHONY: debug-build
debug-build: $(DEBUG_OUTPUT_FILES)

.PHONY: debug-release
debug-release: $(DEBUG_ZIP_TARGET)

$(DEBUG_ZIP_TARGET): $(DEBUG_OUTPUT_FILES) $(INCLUDED_FILES)
	@rm -f $(DEBUG_ZIP_TARGET)
	@rm -rf $(STAGING_ROOT)
	@mkdir -p $(RELEASE_ROOT) $(STAGING_ROOT)
	@cp -t $(STAGING_ROOT) $(INCLUDED_FILES)
	@cd $(STAGING_ROOT) && zip -rq $(DEBUG_ZIP_TARGET) *

$(DEBUG_OUTPUT_FILES): $(CSHARP_FILES)
	@rm -rf $(DEBUG_OUT_DIR) $(INTER_ROOT)
	@dotnet build build.csproj /p:Configuration=$(PROFILE_DEBUG)

#
# Release Builds
#

PROFILE_RELEASE      := Release
RELEASE_OUT_DIR      := $(OUTPUT_ROOT)/$(PROFILE_RELEASE)
RELEASE_ZIP_TARGET   := $(RELEASE_ROOT)/$(ZIP_NAME_PREFIX)-$(MOD_VERSION).zip
RELEASE_OUTPUT_FILES := $(RELEASE_OUT_DIR)/$(ASSEMBLY_NAME).dll

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
# File Targets
#

.tools/jq:
	@mkdir -p .tools
	@wget https://github.com/jqlang/jq/releases/download/jq-1.7.1/jq-linux-amd64 -O .tools/jq

$(STAGING_ROOT)/modicon.png: $(STAGING_ROOT)
	@cp assets/icon-128.png "$@"

$(STAGING_ROOT)/modinfo.json: assets/templates/modinfo.base.json $(STAGING_ROOT)
	@$(JQ) '.modid = "$(MOD_ID)" | .version = "$(MOD_VERSION)" | .name = "$(ASSEMBLY_NAME)"' $< > $@

$(RELEASE_ROOT):
	@mkdir -p "$@"

$(STAGING_ROOT):
	@mkdir -p "$@"
