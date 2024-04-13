#!/bin/bash

REPO=$1   # e.g., Atlus-Script-tools
BRANCH=$2 # e.g., psy/evt-patched-v1.0
HEAD=$3   # e.g., master

# this all assumes we've already done...
## cd external
## git submodule add https://github.com/${AUTHOR}/${REPO}.git
## cd ${REPO}
## git checkout -b ${BRANCH}
# ...and made + committed (but probably not remotely pushed) any changes

# assumes we're running from top-level of EVT repo
# go into external directory + go into repo
cd external/${REPO}
echo "Making patch for ${REPO}..."
# get rid of uncommitted changes, you animal
git stash
# pull from HEAD
git checkout ${HEAD}
git pull
# check out branch with changes
git checkout ${BRANCH}
# create patch, again assuming HEAD is master
git format-patch master --stdout > ../../patches/${REPO}.patch
echo "...Made patch for ${REPO}!"
