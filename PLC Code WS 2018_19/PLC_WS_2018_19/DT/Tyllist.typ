(*
NDTE: 11
NCPE: 47
NDME: 9
*)
2 0	User_Types\User_Types	ARR_TCP_DATA_B_0_4	1024	1	USER	ARRAY	BYTE	17			
2 0			0	11	
2 0	sys_flag_types\sys_flag_	Task_Name_Typ	1025	1	USER	ARRAY	BYTE	17			
2 0			0	9	
4 0	sys_flag_types\sys_flag_	Extended_Task_Info	1026	17	USER	STRUCT				
5 0		TaskName	Task_Name_Typ	1025	0	
6 0		TaskPrio	INT	3	0	
7 0		undocumented_0	INT	3	0	
8 0		TaskPeriod	INT	3	0	
9 0		TaskStack	INT	3	0	
10 0		unused_1	INT	3	0	
11 0		TaskWatchdog	INT	3	0	
12 0		undocumented_2	INT	3	0	
13 0		undocumented_3	INT	3	0	
14 0		undocumented_4	INT	3	0	
15 0		CurDuration	INT	3	0	
16 0		MinDuration	INT	3	0	
17 0		MaxDuration	INT	3	0	
18 0		undocumented_5	INT	3	0	
19 0		CurDelay	INT	3	0	
20 0		MinDelay	INT	3	0	
21 0		MaxDelay	INT	3	0	
26 0	sys_flag_types\sys_flag_	Task_Name_eCLR	1027	1	USER	ARRAY	BYTE	17			
26 0			0	35	
28 0	sys_flag_types\sys_flag_	Task_Info_eCLR	1028	30	USER	STRUCT				
29 0		TaskStack	INT	3	0	
30 0		TaskPrio	INT	3	0	
31 0		TaskPeriod_us	DINT	4	0	
32 0		TaskWatchdog_us	DINT	4	0	
33 0		TaskPeriod	INT	3	0	
34 0		TaskWatchdog	INT	3	0	
35 0		MinDuration_us	DINT	4	0	
36 0		MaxDuration_us	DINT	4	0	
37 0		CurDuration_us	DINT	4	0	
38 0		MinDelay_us	DINT	4	0	
39 0		MaxDelay_us	DINT	4	0	
40 0		CurDelay_us	DINT	4	0	
41 0		MinDuration	INT	3	0	
42 0		MaxDuration	INT	3	0	
43 0		CurDuration	INT	3	0	
44 0		MinDelay	INT	3	0	
45 0		MaxDelay	INT	3	0	
46 0		CurDelay	INT	3	0	
47 0		unused_1	DINT	4	0	
48 0		unused_2	DINT	4	0	
49 0		unused_3	DINT	4	0	
50 0		unused_4	DINT	4	0	
51 0		unused_5	DINT	4	0	
52 0		unused_6	DINT	4	0	
53 0		unused_7	DINT	4	0	
54 0		unused_8	DINT	4	0	
55 0		unused_9	DINT	4	0	
56 0		TNameMaxSize	INT	3	0	
57 0		TNameSize	INT	3	0	
58 0		TaskName	Task_Name_eCLR	1027	0	
63 0	sys_flag_types\sys_flag_	PND_IO_32	1029	1	USER	ARRAY	BYTE	17			
63 0			0	31	
64 0	sys_flag_types\sys_flag_	PND_IO_64	1030	1	USER	ARRAY	BYTE	17			
64 0			0	63	
65 0	sys_flag_types\sys_flag_	PND_IO_128	1031	1	USER	ARRAY	BYTE	17			
65 0			0	127	
66 0	sys_flag_types\sys_flag_	PND_IO_256	1032	1	USER	ARRAY	BYTE	17			
66 0			0	255	
67 0	sys_flag_types\sys_flag_	PND_IO_512	1033	1	USER	ARRAY	BYTE	17			
67 0			0	511	
68 0	sys_flag_types\sys_flag_	Redundancy_OPC_Struct	1034	1	USER	ARRAY	INT	3			
68 0			0	2	
