#!/bin/bash

# make sure submodules are set up
git submodule update --init --recursive

# apply batches to submodules
./scripts/apply_patches.sh;

# install other dependencies
sudo apt-get install libvlc-dev vlc

# uncomment or run these separately if still having issues with sounds
#sudo apt-get install gtk-sharp2
#sudo apt-get install libx11-dev
