Wallmart Kaggle Competition


1 - Load data
2- Exploratory data Analysis
	- Density per category
	- Density per Store
	- Density per State
	- Best and Wrost performers: store 
	- Best and Wrost performers: state
	- Best and Wrost performers: Product
	
Feature Engineering
3 - Clustering for products
	-> Possible binning
	-> Lags
4 -




	
	Submission File
Each row contains an id that is a concatenation of an item_id and a store_id, which is either validation 
(corresponding to the Public leaderboard), or evaluation (corresponding to the Private leaderboard). 
You are predicting 28 forecast days (F1-F28) of items sold for each row. For the validation rows, this corresponds to d_1914 - d_1941, 
and for the evaluation rows, this corresponds to d_1942 - d_1969. 
(Note: a month before the competition close, the ground truth for the validation rows will be provided.)