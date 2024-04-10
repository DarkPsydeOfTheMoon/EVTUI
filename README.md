# EVTUI

## Setup

```
# 1. Clone and enter repository
git clone https://github.com/DarkPsydeOfTheMoon/EVTUI.git
cd EVTUI

# 2. Configure repository and branch
git config user.email "you@example.com"
git config user.name "Your Name
git checkout psy/initial-project-structure

# 3. Configure and patch submodules
git submodule update --init
git submodule foreach --recursive git config user.email "you@example.com"
git submodule foreach --recursive git config user.name "Your Name"
./scripts/apply_patches.sh;
```
