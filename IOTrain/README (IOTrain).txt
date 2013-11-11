IOTrain:
--------
NOTE: Contains a lot of outdated code, and it expects two endpoint wires.  The only part that should be kept is IOTrain.cs and BPTrain.cs.  The rest of the .cs files should be replaced with newer versions from CircuitRec or TestCircuitRec, or better yet, just reference CircuitRec and TestCircuitRec.

Overview
The main purpose of this program is to train a backpropagation network (using a multilayered perceptron) that is used to identify input, output, and internal wires in a circuit sketch.  The input is a labeled XML file, and the output is either a text file that serves as the training data for one diagram, or a text file that represents the weights of the neurons in a backpropagation network.

Operation Modes
When the program is started, there is a prompt to type ‘runbp’ to start training the backpropagation network.  If anything but ‘runbp’ is entered, another prompt comes up that asks for a labeled XML file.  The training data generation is the first mode, and the backpropagation training is the second mode.

For the first mode, a labeled XML file must be entered after the second prompt (just do not type ‘runbp’ at the first prompt), and the file has to be located in the data/Test Data folder in the IOTrain folder.  This can be changed at the top of the code where the XML file is read.  Next, the XML is placed in a temporary Sketch.Sketch variable, then the shapes in it are placed in an array of Sketch.Shape.  An object of the Circuit class is created, whose constructor takes in all of the shapes.  The bounding box (top left and bottom right coordinates) are found, where the top left coordinate is the origin (0,0), and the number of gates, wires, and labels are found.  Then, a loop goes though all the shapes, and based on their types (wire, label, AND, NOR, etc.), it creates new objects of the Wire, Gate, or Label class.  Types that are not wires, gates, or labels are ignored.  When wires are created, the top-left and bottom-right coordinates are stored (currently getting incorrect information), the points and substrokes are stored, and the endpoints are found.  When gates are created, the top-left coordinate, as well as width and height, the substrokes, ID, and the type of gate are stored.  When labels are created, the points are stored in a Sketch.Point array and in an ArrayList, and the substrokes are also stored.  Also, names such as “wireX” and “gateX” are assigned to each wire and gate, where X is incremented from 0 until one minus the number of wires/gates found.  This is used to identify the wires and gates when the circuits are plotted.
Next, the features are extracted from the sketch.  The distance of both endpoints from the four perimeter lines are found using the WireDistToPerim method in the Wire class.  This contributes a total of eight features.  The distance of both endpoints to the nearest gate point is found using the DistWireToGate method in the Wire class, which contributes two more features.  The distance of both endpoints to the nearest wire point besides the current wire is found using the WireDistToWire method in the Wire class, which contributes two more features.  Finally, the distance of any wire point to the closest label is found using the DistToLabel method in the Wire class, which contributes one more feature, for a total of 13 features.
Third, the data is normalized before it is written as training data.  The distance to the left and right perimeter lines is divided by the total width of the circuit.  The distance to the top and bottom perimeter lines is divided by the total height of the circuit.  The distance to the nearest gate is divided by the largest distance to the nearest gate out of all the wires, and the distance to the nearest wire is divided by the largest distance to the nearest wire out of all the wires.
Fourth, the circuit is plotted in a Windows Form using a class library called ZedGraph.  Documentation for this is provided at http://zedgraph.org/wiki/index.php?title=Main_Page and http://www.codeproject.com/csharp/zedgraph.asp.  Wire names are plotted at the endpoints of the wires for easier identification of the wires.  Also, if show point values is selected from the right click menu, the names of the wires pops up in a tool tip if the wire is hovered over.  The names of the wires along with whether they are internal or external should be recorded on a separate piece of paper for the next part.
After closing the form window, the final part of the program starts.  streamwriter is used to write a training file (in the form of a .in file) for each individual sketch.  It starts the file with “3 13”, which gives the number of output dimensions, and the 13 corresponds to the number of input/feature dimensions.  On the next lines, the normalized features will be written.  Prompts come up asking whether each wire is an input (enter 1 into prompt), output (enter 2), or internal (enter 3).  If 1 is entered, 1 0 0 will be put at the beginning of the line before the normalized features, if 2 is entered 0 1 0 will be put at the beginning of the line, and if 3 is entered 0 0 1 will be entered at the beginning of the line.  This takes the first endpoint to the be endpoint that has the earliest timestamp in the XML.  The filename will be the same as the name of the labeled XML that was entered in the beginning.
This training file can then be incorporated into a larger training file with more wires.  Right now, there is one called trainingdata.in.

The second mode is the backpropagation training mode.  This utilizes a multilayered perceptron written by Aaron Wolin, and it is run through the BPTrain.cs file.  This requires an input .in file and a network .nn file.  The quickest network that has been found for this data has 2 hidden layers where the first hidden layer has 7 neurons with tangent sigmoidal activation functions, the second hidden layer has 13 neurons with tangent sigmoidal activation functions, and 3 linear output nodes.  Currently, this is named ‘quickest.nn’.
Just type ‘runbp’ at the first prompt, then all the training data will appear with desired outputs and the inputs.  Press any key, and the training will begin.  It will stop when the maximum number of epochs has been reached or the weights have converged to give a mean square error less than the maximum specified.  The results is written to ioweights.bp which will be in the IOTrain/bin/Debug folder.
The place to change all of these parameters is in the following line of code at the top of IOTrain.cs:

string[] inputargs = new string[]{"-i","ex1.1.labeled.in","-nn","quickest.nn","-test","fulltrain300.bp","-l","350000","-e",".0000001","-m","0","-n",".006"};

BackProp.BPTrain(inputargs);

The text in quotes after “-i” is the input file, the text after “-nn” is the network file that describes the neural network, the text that follows “-test” is the file with the weights for all the neurons, the text that follows “-l” is the maximum number of epochs, the text that follows “-e” is the maximum allowed mean square error, and the text that follows “-n” is the learning rate.  Changing the learning rate can have major effects on how well the network trains.  So far, 0.006 was found to give good results.  The “-test” and the text following should be taken out if a test input is not being run.

Procedure for creating training data
1.Press “ENTER” at the first prompt
2.Enter labeled XML file name without the .xml at the end
3.Various debug messages appear, followed by a Windows form
4.Write down the classification for each wire (1 for input, 2 for output, 3 for internal).  The wire names are at one of the endpoints for the wire, and if ‘Show Point Values’ is selected from the right click menu, when the wires are hovered over, the names are shown.  Also, check to make sure the endpoints for each wire (shown in blue) are correct for each wire
5.Close the Windows form
6.Enter the correct classification (as described above) for each wire at the prompts
7.The training file is generated in the IOTrain/bin/Debug folder!

Procedure for training the neural network
1.Have the correct training file entered after “-i” for the input arguments to the BackProp.BPTrain method (see operation modes above)
2.Make sure there is no “-test” in the input arguments to the BackProp.BPTrain method
3.Type ‘runbp’ at the first prompt
4.All of the training vectors will be display; press any key
5.Training begins (this may take while)
6.After training ends a file called ‘ioweight.bp’ will be written with the weights of the network

Procedure for testing the neural network
1.Have the test file (.in file) entered after the “-i” in the input arguments to the BackProp.BPTrain method
2.Have “-test” and the network weights file (i.e. ioweights.bp) in the input arguments to the BackProp.BPTrain method
3.Type ‘runbp’ at the first prompt
4.Press any key
5.The output of the neural network, the desired output, and the translated classifications (1 for input, 2 for output, 3 for internal) will be shown for each test vector along with text that shows if the above vector was an error.  The Math.Round function is used, so it is possible two outputs will be rounded to the same value, which is counted and displayed at the end as a rounding error.  Also, if there was an error in the desired inputs (somehow something like 1 1 0 was put in as the desired), the error are counted as well.  If any of these two errors occur, the translated output is a 4 for rounding error and 5 for desired input error


Files:
------

1) BPTrain.cs: runs training of the backpropagation network.

2) Circuit.cs: does not need to exist once the code is updated.

3) DetermineIO: does not need to exist once the code is updated.

4) Form1.cs: should be replaced with version from TestCircuitRec.  Displays the recognized circuit in a ZedGraph form.

5) Gate.cs: does not need to exist once the code is updated.

6) IOTrain.cs: used to write training data.

7) Label.cs: needs to be replaced with new code from CircuitRec or just reference CircuitRec.

8) TestCodeForNeuralNet.cs: backup of changes to the NeuralNets train function. Can be deleted.

9) Wire.cs: needs to be replaced with new code from CircuitRec or just reference CircuitRec.

10) fulltrain462_17.bp: Represents trained 17 feature neural net.

11) fulltrain467_13.bp: Represents trained 13 feature neural net.

12) Documentation: contains IOTrain documentation and the IOTrain part of the Spring 2007 research presenation.

13) Test Data: Holds test XML files for CircuitRec/TestCircuitRec (has the Raw XML, files used to test the neural net, the SwitchNameType program, and new data that has not been converted yet)