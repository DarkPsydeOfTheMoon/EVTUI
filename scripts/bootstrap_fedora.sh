#!/bin/bash

# make sure submodules are set up
git submodule update --init --recursive

# apply patches to submodules
./scripts/apply_patches.sh

# install other dependencies
sudo dnf install vlc-devel vlc

# uncomment or run these separately if still having issues with sounds
#sudo dnf install gtk-sharp2
#sudo dnf install libX11-devel
