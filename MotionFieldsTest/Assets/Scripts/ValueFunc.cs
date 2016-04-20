﻿using UnityEngine;
using System.Collections.Generic;

namespace AnimationMotionFields{


	public class ValueFunc {

		//TODO: make dictionary for precomputedTable
		
		public float ComputeReward(float[] currentPos){
			//Calculates the reward for a specific motionstate

			//frst, get the current task array for the motionstate
			//simultaneously calculate immediate reward
			int tasklength = TaskArrayInfo.TaskArray.Count();
			float immediateReward = 0.0f;
			float[] tasks = new float[tasklength];
			for(int i = 0; i < tasklength; i++){
				tasks[i] = TaskArrayInfo.TaskArray[i].DetermineTaskValue(currentPos);
				immediateReward += TaskArrayInfo.TaskArray[i].CheckReward (tasks [i]);
			}

			//calculate continuousReward
			float continuousReward = RewardLookup(currentPos, tasks);

			return immediateReward + continuousReward;
		}

		public float RewardLookup(float[] currentPos, float[] Tasks){
			//get continuous reward at specified motionstate/task array from weighted avg of nearest precalculated states

			//get nearest neighbors to motionstate
			//need to call NearestNeighbor in MFC, cant access it! 
			List<MotionFieldController.NodeData> neighbors = new List<MotionFieldController.NodeData> ();
			//need to call GenerateWeights in MFC, cant access it!
			double[] neighbors_weights = MotionFieldController.GenerateWeights(currentPos, neighbors);

			//generate matrix of closeby precalculated task arrays near Tasks
			List<List<float>> nearest_vals = new List<List<float>> ();
			for(int i=0; i < Tasks.Length(); i++){
				//TODO: if Tasks[i] is the min or max, values added could e out of range. add a check
				List<float> nearest_val = new List<float> ();
				float interval = (TaskArrayInfo.TaskArray [i].max - TaskArrayInfo.TaskArray [i].min) / TaskArrayInfo.TaskArray [i].numSamples;
				nearest_val.Add (Mathf.Floor ((Tasks [i] - TaskArrayInfo.TaskArray [i].min) / interval) * interval + TaskArrayInfo.TaskArray [i].min);
				nearest_val.Add(Mathf.Floor((Tasks [i] - TaskArrayInfo.TaskArray [i].min) / interval) * (interval+1) + TaskArrayInfo.TaskArray [i].min);
				nearest_vals.Add (nearest_val);
			}

			//turn the above/below vals for each task into 2^Tasks.Length() task arrays, each of which exists in precalculated dataset
			List<List<float>> taskMatrixCurrent = MotionFieldUtility.CartesianProduct(nearest_vals);
			List<List<float>> taskMatrixCurrent_weights = new List<List<float>> (); //TODO: make helper func to get these weights

			//get matrix of neighbors x tasks. The corresponding weight matrix should sum to 1.
			List<List<float>> taskNeighbors = new List<List<float>> ();
			List<float> taskNeighbors_weights = new List<float> ();
			for (int i = 0; i < neighbors.Count(); i++){
				for (int j = 0; j < taskMatrixCurrent.Count(); j++){
					taskNeighbors.Add (neighbors [i].Concat (taskMatrixCurrent [j]).ToList ());
					taskNeighbors_weights.Add (neighbors_weights [i] * taskNeighbors_weights [j]);
				}
			}

			//do lookups in precomputed table, get weighted sum
			float continuousReward = 0.0f;
			for(int i = 0; i < taskNeighbors.Count(); i++){
				continuousReward += precomputedTable(taskNeighbors[i])*taskNeighbors_weights[i];
			}

			return continuousReward;
		}
	} 
}
