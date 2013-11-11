Test Data:
----------

Holds test XML files for CircuitRec/TestCircuitRec (has the Raw XML, files used to test the neural net, the SwitchNameType program, and new data that has not been converted yet).

.switched.edit.xml files: labeled; wires have more than two endpoints
.switched.xml files: labeled; wires have only two endpoints
.xml files: unlabeled

Files:
------

1) RawXML/AlreadyLabeled: files in here have counterparts in Test Data that are labeled

2) New Data: has not been used to test yet (use to test neural net)

3) SwitchNameType: switches the Name and Type fields for files that were labeled with the old labeler (new labeler puts name and type fields in the correct place)

4) Used to Test NeuralNet: these files are reserved to test the neural net

5) Files in Test Data: used for main testing and many were used to train the neural net, most are labeled so that wires only have two endpoints, so DO NOT USE ANYMORE

6) Multiple endpoints: files here are labeled so that wires can have greater than two endpoints