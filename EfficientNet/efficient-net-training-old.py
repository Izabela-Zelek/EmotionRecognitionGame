"""
Bhattiprolu, S. (2021, October 13). 239 - Deep Learning training for facial emotion detection [Video]. YouTube. https://www.youtube.com/watch?v=P4OevrwTq78&t=11s

Changed 'validation' to 'test' for easy understanding
"""
from tensorflow.keras.preprocessing.image import ImageDataGenerator 
from tensorflow.keras.models import Sequential
from tensorflow.keras.layers import Dense,Dropout,Flatten
from tensorflow.keras.layers import Conv2D,MaxPooling2D
import os
from matplotlib import pyplot as plt
import numpy as np
import random

IMG_HEIGHT = 48
IMG_WIDTH = 48
batch_size = 32

#Project was running in wrong directory, changed directory to the project folder
script_dir = os.path.dirname(os.path.abspath(__file__))
os.chdir(script_dir)

train_data_dir = 'data/train/'
test_data_dir = 'data/test/'

train_datagen = ImageDataGenerator(
                    rescale=1./255, 
                    rotation_range=30,
                    shear_range=0.3,
                    zoom_range=0.3,
                    horizontal_flip=True,
                    fill_mode='nearest')

test_datagen = ImageDataGenerator(rescale=1./255)

train_generator = train_datagen.flow_from_directory(
                    train_data_dir,
                    color_mode='grayscale',
                    target_size=(IMG_HEIGHT,IMG_WIDTH),
                    batch_size=batch_size,
                    class_mode='categorical',
                    shuffle=True)

test_generator = test_datagen.flow_from_directory(
                    test_data_dir,
                    color_mode='grayscale',
                    target_size=(IMG_HEIGHT,IMG_WIDTH),
                    batch_size=batch_size,
                    class_mode='categorical',
                    shuffle=True)

#Defining labels to allow for easier console reading
class_labels=['Angry','Disgust','Fear','Happy','Neutral','Sad','Surprise']
img,label = train_generator.__next__()

import random
