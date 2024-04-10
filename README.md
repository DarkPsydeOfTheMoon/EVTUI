# EVTUI

## Setup

Setup assumes you're initializing from a command line with `bash` or a similar shell.

Before starting, if you don't have a global git user and/or email but would like to, configure them like so:

```
git config --global user.email "you@example.com"
git config --global user.name "Your Name
```

If you do have global credentials in place, the setup process is as follows:

```
# 1. Clone and enter repository
git clone https://github.com/DarkPsydeOfTheMoon/EVTUI.git
cd EVTUI

# 2. Configure repository and branch
git checkout psy/initial-project-structure

# 3. Configure and patch submodules
git submodule update --init
./scripts/apply_patches.sh;
```

If you'd rather not create global credentials, set up with repository-specific credentials like so:

```
# 1. Clone and enter repository
git clone https://github.com/DarkPsydeOfTheMoon/EVTUI.git
cd EVTUI

# 2. Configure repository and branch
git config user.email "you@example.com"
git config user.name "Your Name
# NOTE: get rid of this line before merging PR
git checkout psy/initial-project-structure

# 3. Configure and patch submodules
git submodule update --init
git submodule foreach --recursive git config user.email "you@example.com"
git submodule foreach --recursive git config user.name "Your Name"
./scripts/apply_patches.sh;
```
