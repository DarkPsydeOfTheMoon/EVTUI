#!/bin/bash

# these probably won't change
USER="psy"
REPO="Atlus-Script-Tools"
MAINBRANCH="master"
# these will probably change over time for versioning
SOURCEBRANCH="evt-patched-v1.0"
TARGETBRANCH="evt-patched-v1.1"

# this all assumes we've already done...
#cd submodules
#git submodule add https://github.com/tge-was-taken/Atlus-Script-Tools.git
#git checkout -b ${USER}/${SOURCEBRANCH}
# ...and made + committed (but probably not remotely pushed) any changes

# assumes we're running from top-level of EVT repo
cd submodules/${REPO}

# enter main/master branch but don't update
git checkout ${MAINBRANCH}
# make patch from old version
git format-patch ${USER}/${SOURCEBRANCH} --stdout > ../patches/${SOURCEBRANCH}.patch

# update to latest for main/master
git pull
# make new patching branch
git checkout -B ${USER}/${TARGETBRANCH}
# update new branch based on patch
git am ../patches/${SOURCEBRANCH}.patch
