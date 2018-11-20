(*@PROPERTIES_EX@
TYPE: POU
LOCALE: 0
IEC_LANGUAGE: ST
PLC_TYPE: independent
PROC_TYPE: independent
*)
(*@KEY@:DESCRIPTION*)

(*@KEY@:END_DESCRIPTION*)
FUNCTION_BLOCK Select_Position_Strg_Ver

(*Group:Default*)


VAR_INPUT
	SVPosCent :	BOOL;
	SVPosDown :	BOOL;
	SVPosUp :	BOOL;
END_VAR


VAR_EXTERNAL
	Q_0_2_1_1 :	BOOL;(*Motor2 Bit1*)
	Q_0_2_1_3 :	BOOL;(*Motor2 Bit2*)
	Q_0_2_1_4 :	BOOL;(*Motor2 Bit3*)
	Q_0_2_1_6 :	BOOL;(*Motor2 Bit4*)
END_VAR


VAR_INPUT
	SVRef :	BOOL;(*ReferenzFahrt starten*)
	SVRelPl :	BOOL;(*Release Position Plate*)
	SVRelRo :	BOOL;(*Release Position Robot*)
	SVStop :	BOOL;(*Stop*)
END_VAR


(*@KEY@: WORKSHEET
NAME: Select_Position_Strg_Ver
IEC_LANGUAGE: ST
*)
IF SVPosDown=TRUE THEN;
Q_0_2_1_1:=TRUE;
Q_0_2_1_3:=FALSE;
Q_0_2_1_4:=FALSE;
Q_0_2_1_6:=FALSE;
ELSIF SVPosCent=TRUE THEN;
Q_0_2_1_1:=FALSE;
Q_0_2_1_3:=TRUE;
Q_0_2_1_4:=FALSE;
Q_0_2_1_6:=FALSE;
ELSIF SVPosUp=TRUE THEN;
Q_0_2_1_1:=TRUE;
Q_0_2_1_3:=TRUE;
Q_0_2_1_4:=FALSE;
Q_0_2_1_6:=FALSE;
ELSIF SVStop=TRUE THEN;
Q_0_2_1_1:=FALSE;
Q_0_2_1_3:=FALSE;
Q_0_2_1_4:=TRUE;
Q_0_2_1_6:=FALSE;
ELSIF SVRef=TRUE THEN;
Q_0_2_1_1:=TRUE;
Q_0_2_1_3:=FALSE;
Q_0_2_1_4:=TRUE;
Q_0_2_1_6:=FALSE;
ElSIF SVRelPl=TRUE THEN;
Q_0_2_1_1:=TRUE;
Q_0_2_1_3:=TRUE;
Q_0_2_1_4:=TRUE;
Q_0_2_1_6:=FALSE;
ElSIF SVRelRo=TRUE THEN;
Q_0_2_1_1:=FALSE;
Q_0_2_1_3:=FALSE;
Q_0_2_1_4:=FALSE;
Q_0_2_1_6:=TRUE;
ELSE (* Huefner: Adding ELSE Condition, setting all to FALSE if no position is given *)
Q_0_2_1_1:=FALSE;
Q_0_2_1_3:=FALSE;
Q_0_2_1_4:=FALSE;
Q_0_2_1_6:=FALSE;
END_IF;

(*@KEY@: END_WORKSHEET *)
END_FUNCTION_BLOCK
