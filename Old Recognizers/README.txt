# -------------------------------------------------------------------------- #
################################ RECOGNIZERS #################################
# -------------------------------------------------------------------------- #

Recognizers is a project that serves as a general framework for recognition.

# -------------------------------------------------------------------------- #

IRecognizer is the interface that all recognizers should implement.
Recognizers implementing this interface can be used in TestRig, SketchPanel,
or any of our other modern products.

Recognizer is a runtime-selectable class that implements IRecognizer and
lets you use different algorithms. It currently supports GATE, PARTIAL_GATE,
WGL, and CONGEAL, as specified through the Recognizers.Algorithm enum. It's
pretty straightforward to add a new Recognizer here.

Results is a wrapper class for holding the output of a recognizer. Probably,
you'll use Add and BestLabel. 

--- List of Recognizers ---
 
 * CongealRecognizer: see the Recognition/Congeal README file for
   information on how congealing works. This class wraps the congealing
   algorithm in an IRecognizer. This recognizer is multithreaded and can
   scale up to the number of gate classes to recognize (generally, 8
   threads). However, multithreading is only enabled in RELEASE mode. For
   more information, see the comments in the initThreads() function.
 * GateRecognizer: A SVM-based image recognizer for gates. Sort of black
   magic, and highly unreliable.
 * PartialGateRecognizer: Another SVM-based recognizer, this one tuned for
   gate parts (lines, arcs, etc.). Also unreliable.
 * DollarRecognizer: A feature-based recognizer originally developed by
   Jacob Wobbrock, Andrew Wilson, and Yang Li at the University of
   Washington, Seattle and Microsoft Research. See "Gestures without
   libraries, toolkits, or training: a $1 recognizer for user interface
   prototypes" for more information.
 * Rubine: another feature-based recognizer. See Dean Rubine. "Specifying
   Gestures by Example"
 * ShapeHereRecognizer: The null recognizer.

--- Other Files ---
 
 * FeatureFunctions: Some miscellaneous calculations that probably should be
   done in Featurefy, but aren't.
 * Colorizer: Convert output to a color for use in the Labeler or
   SketchPanel
