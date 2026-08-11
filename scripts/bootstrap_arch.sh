#!/bin/bash

# make sure submodules are set up
git submodule update --init --recursive

# apply batches to submodules
./scripts/apply_patches.sh

# install other dependencies
sudo pacman -S --needed vlc libx11

# uncomment if you don't have yay (if unavailable on your distro's repo visit https://aur.archlinux.org/packages/yay and download the snapshot, then run makePKG)
# sudo pacman -S --needed yay

yay gtk-sharp-2
