Welcome to the AirBrain.org project!

# Introduction
This project provides the source code to facilitate the creation of machine learning algorithms that can be used for the analysis of aerial video.  The first use case for this project is to support early fire detection from the online analysis of aerial images.

Generating training data that can be used in the context of detecting a natural or unnatural disaster would require the collection of aerial video associated with an actual disaster. In the case of early fire detection, this would require the collection of images in a controlled setting, and even then it is unlikely that the images collected would be sufficient in number to encompass the wide variety of terrain required to adquately train a deep network.  

To address this issue, we have used the Unity game engine to create realistic aerial landscapes from satellite images, and added smoke in various forms.  A flyover video of one such terrain is linked below.  The Unity Assets used to generate the scene is listed below in the Appendix.

[TODO: YouTube link to flyover video]

The remainder of this document provides instructions for how to use the C# scripts in the traingen project to automatically generate images at random positions, containing smoke sequences that have been randomly modified.

# Long Term Project Plan
## TrainGen
Additional enhancements are planned to allow TrainGen to randomly vary other aspects of the scene, including:
- Automatic creation of smoke on the terrain at a specified density, at random locations
- Randomization of the weather and lighting
- Automatic selection of predefined terrains after the required number of images for another terrain have been generated 
## Use of TrainGen Images to Seed a Generative Adversarial Network (GAN)
## Deployment on a UAS
## Integration with Unity's ML-Agents Tool
![](docs/ml-agents_integration.jpg "ML-Agents Integration")

[TODO: Block Diagram]

# Unity Scripts

[TODO: YouTube link to demonstration of TrainGen]

# Step-by-step Guide to Using TrainGen in Your Unity Project


# Call for Collaboration
The primary mission of AirBrain.org is to enable the creation of open source machine learning solutions for use in the aerial domain. This cannot happen without the involvement of many other collaborators from various domains, some of which are listed below. If you would like to contribute to this project, or have any ideas on how we can begin collaboration with any of the entities listed below, please post to the Issues section of github for this project. If you would prefer to communicate privately send a message to airbrain.org@gmail.com.

- Universities and government authorities conducting research on early fire detection
- Researchers investigating the use of simulation data to train machine learning algorithms for aerial observation
- Artists and game developers interested in creating simulators for other types of aerial terrain
- Anyone else in need of more data for their aerial machine learning application

# Appendix
This section contains a list of the resources used to develop and test the TrainGen scripts. Thanks to the Unity ecosystem, so much can be accomplished in a short period of time!

## Development Tools

## Unity Assets Used
