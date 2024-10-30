# Copyright (c) 2023 DeNA Co., Ltd.
# This software is released under the MIT License.

PACKAGE_HOME?=$(shell dirname $(realpath $(lastword $(MAKEFILE_LIST))))
PROJECT_HOME?=$(PACKAGE_HOME)/UnityProject~
BUILD_DIR?=$(PROJECT_HOME)/Build
LOG_DIR?=$(PROJECT_HOME)/Logs
UNITY_VERSION?=$(shell grep '"unity":' $(PACKAGE_HOME)/package.json | grep -o -E '\d{4}\.[1-4]').$(shell grep '"unityRelease":' $(PACKAGE_HOME)/package.json | grep -o -E '\d+[abfp]\d+')
PACKAGE_NAME?=$(shell grep -o -E '"name": "(.+)"' $(PACKAGE_HOME)/package.json | cut -d ' ' -f2)

# Code Coverage report filter (comma separated)
# see: https://docs.unity3d.com/Packages/com.unity.testtools.codecoverage@1.2/manual/CoverageBatchmode.html
PACKAGE_ASSEMBLIES?=$(shell echo $(shell find $(PACKAGE_HOME) -name "*.asmdef" | grep -v -E "\/UnityProject~\/" | sed -e s/.*\\//\+/ | sed -e s/\\.asmdef// | sed -e s/^.*\\.Tests//) | sed -e s/\ /,/g)
COVERAGE_ASSEMBLY_FILTERS?=$(PACKAGE_ASSEMBLIES),+<assets>,-*.Tests

UNAME := $(shell uname)
ifeq ($(UNAME), Darwin)
UNITY_HOME=/Applications/Unity/HUB/Editor/$(UNITY_VERSION)/Unity.app/Contents
UNITY?=$(UNITY_HOME)/MacOS/Unity
UNITY_YAML_MERGE?=$(UNITY_HOME)/Tools/UnityYAMLMerge
STANDALONE_PLAYER=StandaloneOSX
endif

define ENVIRONMENT_VARIABLES_FOR_ARGUMENT_CAPTURE_TESTS
  STR_ENV=STRING_BY_ENVIRONMENT_VARIABLE
endef

define base_arguments
  -projectPath $(PROJECT_HOME) \
  -logFile $(LOG_DIR)/$(TEST_PLATFORM).log
endef

define test_arguments
  -batchmode \
  -silent-crashes \
  -stackTraceLogType Full \
  -runTests \
  -testCategory "!IgnoreCI" \
  -testPlatform $(TEST_PLATFORM) \
  -testResults $(LOG_DIR)/$(TEST_PLATFORM)-results.xml \
  -testHelperScreenshotDirectory $(LOG_DIR)/Screenshots
endef

define test
  $(eval TEST_PLATFORM=$1)
  mkdir -p $(LOG_DIR)
  $(ENVIRONMENT_VARIABLES_FOR_ARGUMENT_CAPTURE_TESTS) \
  $(UNITY) \
    $(call base_arguments) \
    $(call test_arguments)
endef

define cover
  $(eval TEST_PLATFORM=$1)
  mkdir -p $(LOG_DIR)
  $(ENVIRONMENT_VARIABLES_FOR_ARGUMENT_CAPTURE_TESTS) \
  $(UNITY) \
    $(call base_arguments) \
    $(call test_arguments) \
    -burst-disable-compilation \
    -debugCodeOptimization \
    -enableCodeCoverage \
    -coverageResultsPath $(LOG_DIR) \
    -coverageOptions 'generateAdditionalMetrics;generateTestReferences;dontClear;assemblyFilters:$(COVERAGE_ASSEMBLY_FILTERS)'
endef

define cover_report
  mkdir -p $(LOG_DIR)
  $(UNITY) \
    $(call base_arguments) \
    -batchmode \
    -quit \
    -enableCodeCoverage \
    -coverageResultsPath $(LOG_DIR) \
    -coverageOptions 'generateHtmlReport;generateAdditionalMetrics;generateAdditionalReports;assemblyFilters:$(COVERAGE_ASSEMBLY_FILTERS)'
endef

.PHONY: usage
usage:
	@echo "Tasks:"
	@echo "  create_project: Create Unity project for run UPM package tests."
	@echo "  remove_project: Remove created project."
	@echo "  clean: Clean Build and Logs directories in created project."
	@echo "  setup_unityyamlmerge: Setup UnityYAMLMerge as mergetool in .git/config."
	@echo "  open: Open this project in Unity editor."
	@echo "  test_editmode: Run Edit Mode tests."
	@echo "  test_playmode: Run Play Mode tests."
	@echo "  cover_report: Create code coverage HTML report."
	@echo "  test: Run test_editmode, test_playmode, and cover_report. Recommended to use with '-k' option."
	@echo "  test_standalone_player: Run Play Mode tests on standalone player."

# Create Unity project for run UPM package tests. And upgrade and add dependencies for tests.
# Required install [openupm-cli](https://github.com/openupm/openupm-cli).
.PHONY: create_project
create_project:
	$(UNITY) \
	  -createProject $(PROJECT_HOME) \
	  -batchmode \
	  -quit
	touch $(PROJECT_HOME)/Assets/.gitkeep
	openupm add -c $(PROJECT_HOME) -f com.unity.test-framework@stable
	openupm add -c $(PROJECT_HOME) -f com.unity.testtools.codecoverage
	openupm add -c $(PROJECT_HOME) -f com.cysharp.unitask
	openupm add -c $(PROJECT_HOME) -f com.nowsprinting.test-helper
	openupm add -c $(PROJECT_HOME) -f com.nowsprinting.test-helper.monkey
	openupm add -c $(PROJECT_HOME) -f com.nowsprinting.test-helper.random
	openupm add -c $(PROJECT_HOME) -ft $(PACKAGE_NAME)@file:../../

.PHONY: remove_project
remove_project:
	rm -rf $(PROJECT_HOME)

.PHONY: clean
clean:
	rm -rf $(BUILD_DIR)
	rm -rf $(LOG_DIR)

.PHONY: setup_unityyamlmerge
setup_unityyamlmerge:
	git config --local merge.tool "unityyamlmerge"
	git config --local mergetool.unityyamlmerge.trustExitCode false
	git config --local mergetool.unityyamlmerge.cmd '$(UNITY_YAML_MERGE) merge -p "$$BASE" "$$REMOTE" "$$LOCAL" "$$MERGED"'

.PHONY: open
open:
	mkdir -p $(LOG_DIR)
	$(ENVIRONMENT_VARIABLES_FOR_ARGUMENT_CAPTURE_TESTS) \
	$(UNITY) $(call base_arguments) &

.PHONY: test_editmode
test_editmode:
	$(call cover,editmode)

.PHONY: test_playmode
test_playmode:
	$(call cover,playmode)

.PHONY: cover_report
cover_report:
	$(call cover_report)

# Run Edit Mode and Play Mode tests with coverage and html-report by codecoverage package.
# If you run this target with the `-k` option, if the Edit/ Play Mode test fails,
# it will run through to Html report generation and return an exit code indicating an error.
.PHONY: test
test: test_editmode test_playmode cover_report

# Run Play Mode tests on standalone player
# Using "test" task instead of "cover" task because the codecoverage package does not support running it on the player.
.PHONY: test_standalone_player
test_standalone_player:
	$(call test,$(STANDALONE_PLAYER))
