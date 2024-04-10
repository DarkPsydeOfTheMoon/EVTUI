#!/bin/bash

# these probably won't change
USER="psy"
# these will probably change over time for versioning
#TARGETBRANCH="evt-patched-v1.1"
TARGETBRANCH="my-local-branch"

# this all assumes patches/${REPO}.patch exists
# ...for every submodule repo

# assumes we're running from top-level of EVT repo
# go into submodules directory
cd submodules
for REPO in $(ls); do
	echo "Applying patch to ${REPO}..."
	# go into repo
	cd ${REPO}
	# update master branch, assuming it's called "master"
	# (a safe assumption for now, but an assumption indeed)
	git checkout master
	git pull
	# make new branch (off of master) to be patched
	git checkout -B ${USER}/${TARGETBRANCH}
	# abandon any prior patching attempts
	git am --abort
	# update new branch based on patch
	git am ../../patches/${REPO}.patch
	# go back into general submodules directory
	cd ..
	echo "...Applied patch to ${REPO}!"
done
