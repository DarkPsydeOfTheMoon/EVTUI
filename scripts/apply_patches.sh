#!/bin/bash

# these probably won't change
#USER="psy"
# these will probably change over time for versioning
#TARGETBRANCH="evt-patched-v1.2"
#TARGETBRANCH="my-local-branch"

# this all assumes patches/${REPO}.patch exists
# ...for every submodule repo

# assumes we're running from top-level of EVT repo
# go into external directory
cd external
for REPO in $(echo "Atlus-Script-Tools" "GFD-Studio"); do
	echo "Applying patch to ${REPO}..."
	# go into repo
	cd ${REPO}
	# get rid of prior patch
	# if you've made other changes, too bad, why are you running this script
	git reset --hard HEAD
	# update master branch, assuming it's called "master"
	# (a safe assumption for now, but an assumption indeed)
	git submodule update --init
	git submodule sync
	#git checkout master
	#git pull
	# make new branch (off of master) to be patched
	#git checkout -B ${USER}/${TARGETBRANCH}
	# update new branch based on patch
	git apply ../../patches/${REPO}.patch
	# go back into general external directory
	cd ..
	echo "...Applied patch to ${REPO}!"
done
