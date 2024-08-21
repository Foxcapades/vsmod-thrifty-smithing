ASSEMBLY_NAME   := $(shell cat build.csproj | tr -d '[:space:]' | sed 's/.*AssemblyName>\([^<]\+\).*/\1/')
MOD_VERSION := $(shell cat modinfo.json | tr -d '[:space:]' | sed 's/.\+version":"//' | cut -d'"' -f1)

BUILD_ROOT   := ${PWD}/$(shell cat Directory.Build.props | tr -d '[:space:]' | sed 's/.*BuildRoot>\([^<]\+\).*/\1/')
INTER_ROOT   := $(BUILD_ROOT)/obj
OUTPUT_ROOT  := $(BUILD_ROOT)/out
STAGING_ROOT := $(BUILD_ROOT)/staging
RELEASE_ROOT := $(BUILD_ROOT)/release

ZIP_NAME_PREFIX := $(shell echo $(ASSEMBLY_NAME) | sed 's/\([A-Z]\)/-\L\1/g;s/^-//')
PROD_ZIP_TARGET  := $(RELEASE_ROOT)/$(ZIP_NAME_PREFIX)-$(MOD_VERSION).zip

TESTING_DATA_PATH := ${PWD}/testing

PROFILE_RELEASE := Release

CSHARP_FILES := $(shell find src -type f -name '*.cs')
BUNDLED_FILES := modinfo.json

#
# Meta Targets
#

.PHONY: default
default:
	@echo
	@echo "Assembly Name  = $(ASSEMBLY_NAME)"
	@echo "Zip Prefix     = $(ZIP_NAME_PREFIX)"
	@echo "Mod Version    = $(MOD_VERSION)"
	@echo
	@echo "Build Root     = $(BUILD_ROOT)"
	@echo "Output Root    = $(OUTPUT_ROOT)"
	@echo "Staging Root   = $(STAGING_ROOT)"
	@echo "Release Root   = $(RELEASE_ROOT)"
	@echo
	@echo "Debug Bundle   = $(DEBUG_ZIP_TARGET)"
	@echo "Release Bundle = $(PROD_ZIP_TARGET)"
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

$(DEBUG_ZIP_TARGET): $(DEBUG_OUTPUT_FILES) $(STAGING_ROOT)/modicon.png $(STAGING_ROOT)/modinfo.json
	@rm -f "$(DEBUG_ZIP_TARGET)"
	@rm -rf "$(STAGING_ROOT)"
	@mkdir -p "$(RELEASE_ROOT)" "$(STAGING_ROOT)"
	@cp -t "$(STAGING_ROOT)" $(DEBUG_OUTPUT_FILES) $(BUNDLED_FILES)
	@cd "$(STAGING_ROOT)" && zip -rq "$(DEBUG_ZIP_TARGET)" *

$(DEBUG_OUTPUT_FILES): $(CSHARP_FILES)
	@rm -rf $(DEBUG_OUT_DIR) $(INTER_ROOT)
	@dotnet build build.csproj /p:Configuration=$(PROFILE_DEBUG)

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

$(STAGING_ROOT)/modicon.png: $(STAGING_ROOT)
	@cp assets/icon-128.png "$@"

$(STAGING_ROOT)/modinfo.json: modinfo.json $(STAGING_ROOT)
	@cp "$<" "$@"

$(RELEASE_ROOT):
	@mkdir -p "$@"

$(STAGING_ROOT):
	@mkdir -p "$@"
