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
Unity Technologies has made available a toolkit to support the creation of machine learning algorithms that interact with the graphical content in a Unity project. Tensor Flow is used for those algorithms that use Deep Learning.  

In the diagram shown below TrainGen is used in a Unity project that uses ML-Agents to communicate with a Generative Adversarial Network (GAN) developed with TensorFlow, and send dynamically generated training images. The feature vector which controls the content of the generated images is formed as the output of the Image Generator network and later sent back to the Unity project to be rendered.  The Image Classifier accepts these rendered images as training input to a classifier network which attempts to classify the renered image.  The output of this classification is used as input to the Image Generator which forms another feature vector for rendering. The objective of the Image Generator network is to create images that the classifier is unable to identify.

# Unity Scripts

This short video sequence demonstrates the use of the TrainGen scripts to automatically move the main camera around designated objects in the scene. In this demonstration the objects are smoke trails representing the early stages of an uncontrolled fire. Each time the camera is moved an image is captured and saved in one of two directories, one reserved for scenes where the object is present, and the other for where it is not. Both types of images are used to train a Deep Learning network and verify correct output when the particular object is not present. The position of the camera and parameters for appearance of the smoke are randomly modified between images using predefined ranges for each value modified.

In the beginning of the video the main camera is shown moving to new positions with short pauses between each movement.  This occurs because the camera movements are triggered when the user presses the right shift key on the keyboard.  Later each movement occurs without being triggered and each new movement occurs without delay.  This generates images as fast as possible while they are saved in their respective directories.  

If a smoke trail appears in the scene, the smoke particles flow for a randomly generated time period before the image is captured.  This allows time for changes in the properties of the particle system used to generate the smoke trail to appear on screen.

[TODO: YouTube link to demonstration of TrainGen]

The next section provides a detailed walk-through of how to integrate the TrainGen scripts into a typical Unity project.  Familiarity with the Unity scripting and the associated IDE are assumed. See the Appendix for additional references.

# Step-by-step Guide to Using TrainGen in Your Unity Project
## Step 1: [LEFT-OFF]


# Call for Collaboration
The primary mission of AirBrain.org is to enable the creation of open source machine learning solutions for use in the aerial domain. This cannot happen without the involvement of many other collaborators from various domains, some of which are listed below. If you would like to contribute to this project, or have any ideas on how we can begin collaboration with any of the entities listed below, please post to the Issues section of github for this project. If you would prefer to communicate privately send a message to airbrain.org@gmail.com.

- Universities and government authorities conducting research on early fire detection
- Researchers investigating the use of simulation data to train machine learning algorithms for aerial observation
- Artists and game developers interested in creating simulators for other types of aerial terrain
- Anyone else in need of more data for their aerial machine learning application

# Appendix
This section contains a list of the resources used to develop and test the TrainGen scripts. Thanks to the Unity ecosystem, so much can be accomplished in a short period of time!

- Unity ML-Agents: https://github.com/Unity-Technologies/ml-agents
- Unity Engine Tutorial: https://unity3d.com/learn/tutorials

## Development Tools

## Unity Assets Used
