Independent Study – Pedestrian Interaction & Eye Gaze Accuracy
This repository contains three Unity environments:

Pedestrian Interaction (branch: main)

Eye Gaze Accuracy Test (branch: accuracy-test)

Facial & Body Tracking (included assets and sample scenes; see Assets/)

Large binaries (models, textures, scenes, etc.) are stored with Git LFS so the repo stays pushable and fast to clone.

Requirements
Unity (exact version in the repo):
Open ProjectSettings/ProjectVersion.txt after cloning to see the exact version (e.g., 2022.3.xx f1).
Install that version in Unity Hub.

Git + Git LFS

Windows (WSL/Ubuntu): sudo apt-get install -y git git-lfs && git lfs install

macOS (Homebrew): brew install git git-lfs && git lfs install

Windows (native): Install Git + Git LFS, then run git lfs install once.

Platform modules (as needed):

Windows Standalone

(Optional) Android module if targeting Quest/OpenXR samples

Cloning (with LFS)
Do not use “Download ZIP” for development. That can include LFS pointers instead of real files. Use git clone + LFS commands below.

A) Pedestrian Interaction (main)
bash
Copy
Edit
# choose a clean folder
git clone https://github.com/Jesse-Ibarra/Independent_Study_Pedestrian_Interaction_Sim.git main-project
cd main-project

# pull large files from LFS
git lfs install
git lfs pull
git lfs checkout   # replace any pointer files in the working tree
B) Eye Gaze Accuracy Test (accuracy-test)
Option 1: clone directly to that branch

bash
Copy
Edit
git clone -b accuracy-test https://github.com/Jesse-Ibarra/Independent_Study_Pedestrian_Interaction_Sim.git accuracy-test
cd accuracy-test
git lfs install
git lfs pull
git lfs checkout
Option 2: one clone, add a worktree (side-by-side folders)

bash
Copy
Edit
cd main-project
git fetch origin accuracy-test
git worktree add ../accuracy-test origin/accuracy-test

cd ../accuracy-test
git lfs pull
git lfs checkout
Open in Unity
Unity Hub → Add the project folder (main-project or accuracy-test).

Use the exact Unity version from ProjectSettings/ProjectVersion.txt.

First open will install packages and rebuild Library (normal).

Open the relevant scene(s) and press Play.

Project layout
bash
Copy
Edit
Assets/           # Source assets, scenes, scripts (what you edit)
Packages/         # Package manifest/lock for Unity Package Manager
ProjectSettings/  # Unity project configuration
UserSettings/     # Per-user editor prefs (ignored by Git)
Library/ Temp/    # Auto-generated (not in Git)
Branches
main – Pedestrian Interaction environment

accuracy-test – Eye Gaze Accuracy Test environment

Switch with:

bash
Copy
Edit
git checkout main
# or
git checkout accuracy-test
Troubleshooting
Zip download shows compile/import errors

Use git clone + git lfs pull + git lfs checkout (ZIPs can contain LFS pointers).

Make sure Unity version matches ProjectVersion.txt.

Packages won’t resolve / compile errors on first open

Close Unity.

Delete: Library/, Temp/, Logs/ (if present).

(If needed) delete Packages/packages-lock.json (do not delete manifest.json).

Reopen in Unity Hub; let it reimport.

Platform errors (OpenXR/Oculus/Input System)

Unity Hub → install the required modules (Windows, Android).

Unity → File → Build Settings… → pick the correct target and click Switch Platform.

Verify LFS files were downloaded

bash
Copy
Edit
git lfs ls-files | wc -l
# Optional: check for leftover pointer text files
grep -R "git-lfs.github.com/spec/v1" -n Assets || echo "No LFS pointers found ✅"
