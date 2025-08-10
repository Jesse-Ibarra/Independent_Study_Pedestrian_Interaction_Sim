# Install Git LFS if you haven't already
git lfs install

# Clone the main project (Pedestrian Interaction Environment)
git clone https://github.com/Jesse-Ibarra/Independent_Study_Pedestrian_Interaction_Sim.git main-project
cd main-project
git lfs pull
git lfs checkout
cd ..

# Clone the Accuracy Test branch
git clone -b accuracy-test https://github.com/Jesse-Ibarra/Independent_Study_Pedestrian_Interaction_Sim.git accuracy-test
cd accuracy-test
git lfs pull
git lfs checkout
cd ..

echo "Both projects have been cloned with full LFS assets. Open them in Unity Hub using the Unity version listed in ProjectSettings/ProjectVersion.txt."
