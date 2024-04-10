#!/bin/bash

# these probably won't change
USER="psy"
# these will probably change over time for versioning
SOURCEBRANCH="evt-patched-v1.0"

# this all assumes we've already done...
## cd submodules
## git submodule add https://github.com/tge-was-taken/Atlus-Script-Tools.git
## cd Atlus-Script-Tools
## git checkout -b ${USER}/${SOURCEBRANCH}
# ...and made + committed (but probably not remotely pushed) any changes

# assumes we're running from top-level of EVT repo
# go into submodules directory
cd submodules
for REPO in $(ls); do
	echo "Making patch for ${REPO}..."
	# go into repo
	cd ${REPO}
	# check out branch with changes
	git checkout ${USER}/${SOURCEBRANCH}
	# create patch, assuming HEAD is master
	# (a safe assumption for now, but an assumption indeed)
	git format-patch master --stdout > ../../patches/${REPO}.patch
	# go back into general submodules directory
	cd ..
	echo "...Made patch for ${REPO}!"
done
