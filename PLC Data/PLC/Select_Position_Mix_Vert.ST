(*@PROPERTIES_EX@
TYPE: POU
LOCALE: 0
IEC_LANGUAGE: ST
PLC_TYPE: independent
PROC_TYPE: independent
*)
(*@KEY@:DESCRIPTION*)

(*@KEY@:END_DESCRIPTION*)
FUNCTION_BLOCK Select_Position_Mix_Vert

(*Group:Default*)


VAR_INPUT
	MPosUp :	BOOL;
	MPosDown :	BOOL;
	MPos6 :	BOOL;
	MPos5 :	BOOL;
	MPos4 :	BOOL;
	MPos3 :	BOOL;
	MPos2 :	BOOL;
	MPos1 :	BOOL;
END_VAR


VAR_EXTERNAL
	Q_0_2_0_0 :	BOOL;(*Motor3 Bit1*)
	Q_0_2_0_2 :	BOOL;(*Motor3 Bit2*)
	Q_0_2_0_1 :	BOOL;(*Motor3 Bit3*)
	Q_0_2_0_3 :	BOOL;(*Motor3 Bit4*)
END_VAR


VAR_INPUT
	MStop :	BOOL;(*Stop*)
	MPosRelRobot :	BOOL;(*Release Position (Robot)*)
	MRef :	BOOL;(*Referenzfahrt*)
END_VAR


(*@KEY@: WORKSHEET
NAME: Select_Position_Mix_Vert
IEC_LANGUAGE: ST
*)
IF MPosDown=TRUE THEN;
Q_0_2_0_0:=TRUE;
Q_0_2_0_2:=FALSE;
Q_0_2_0_1:=FALSE;
Q_0_2_0_3:=FALSE;
ELSIF MPos2=TRUE THEN;
Q_0_2_0_0:=FALSE;
Q_0_2_0_2:=TRUE;
Q_0_2_0_1:=FALSE;
Q_0_2_0_3:=FALSE;
ELSIF MPos1=TRUE THEN;
Q_0_2_0_0:=TRUE;
Q_0_2_0_2:=TRUE;
Q_0_2_0_1:=FALSE;
Q_0_2_0_3:=FALSE;
ELSIF MPosUp=TRUE THEN;
Q_0_2_0_0:=FALSE;
Q_0_2_0_2:=FALSE;
Q_0_2_0_1:=TRUE;
Q_0_2_0_3:=FALSE;  (*Danke Martin*)
(*ELSIF MPos5=TRUE THEN;
Q_0_2_0_0:=TRUE;
Q_0_2_0_2:=FALSE;
Q_0_2_0_1:=TRUE;
Q_0_2_0_3:=FALSE;
ELSIF MPos6=TRUE THEN;
Q_0_2_0_0:=TRUE;
Q_0_2_0_2:=TRUE;
Q_0_2_0_1:=TRUE;
Q_0_2_0_3:=FALSE;*)
ELSIF MRef=TRUE THEN;
Q_0_2_0_0:=FALSE;
Q_0_2_0_2:=TRUE;
Q_0_2_0_1:=FALSE;
Q_0_2_0_3:=TRUE;
ELSIF MStop=TRUE THEN;
Q_0_2_0_0:=TRUE;
Q_0_2_0_2:=FALSE;
Q_0_2_0_1:=FALSE;
Q_0_2_0_3:=TRUE;
ELSIF MPosRelRobot=TRUE THEN;
Q_0_2_0_0:=TRUE;
Q_0_2_0_2:=TRUE;
Q_0_2_0_1:=FALSE;
Q_0_2_0_3:=TRUE;
ELSE
Q_0_2_0_0:=FALSE;
Q_0_2_0_2:=FALSE;
Q_0_2_0_1:=FALSE;
Q_0_2_0_3:=FALSE;
END_IF;
(*@KEY@: END_WORKSHEET *)
END_FUNCTION_BLOCK
