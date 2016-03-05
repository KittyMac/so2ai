using System.Collections.Generic;


namespace SampleAI {
	
	public class AI : ISO2AI {

		//  ****************************  Begin required methods for so2ai interface **************************** 

		static AI() {
			UnityEngine.Debug.Log ("AI static constructor");
		}


		// This name should be globally unique among other SO2 ai
		public string Identifier () {
			return "SampleAI";
		}

		// This is the name displayed to the player when they are selecting AI for their opponents
		public string UserFacingName () {
			return "Sample AI";
		}

		// This is the name displayed to the player when they are selecting AI for their opponents
		public string UserFacingAuthor () {
			return "Rocco Bowling";
		}

		// This is the a longer description displayed to the player when they are selecting AI for their opponents; in other words, explain
		// what this AI is about and why they should consider picking it.  This is a Markdown string.
		public string UserFacingDescription () {
			return "This sample AI will simply explore all planets within range of its home system.  Great for playing against a placebo AI for learning the game!";
		}


		// This method will be called once, usually at the very beginning, to allow for your AI to register their 
		// racial characteristics for this match.

		/// <summary>
		/// This method will be called once, usually at the very beginning, to allow for your AI to register their racial characteristics for this match.
		/// Note that it is important to chec the results of the RegisterOrder order; some game types will restrict the number / amount of racial
		/// attributes allowed
		/// </summary>
		/// <param name="game">The game in which your AI is playing.</param>
		/// <param name="aiEmpire">The empire which your AI represents.</param>
		/// <param name="raceTypes">A list of valid race id strings for this game.</param>
		/// <param name="stockRaces">A list of valid empires (aka the "stock" race configurations); your AI can use these if it wishes, or it can create a custom race.</param>

		public void RegisterEmpire(SOGGame game, SOEEmpire aiEmpire, List<string> raceTypes, List<SOEEmpire> stockRaces) {
			string error;

			// This simple AI will simply grab one of the stock races at random to use
			SOEEmpire randomEmpire = stockRaces [ new System.Random().Next() % stockRaces.Count ];

			SOCOrderRegisterRace.RegisterOrder (aiEmpire, out error, randomEmpire.raceType, "Sample AI", 0, randomEmpire.RaceAttributes);
			if(error != null) {
				UnityEngine.Debug.Log ("error: " + error);
			}
		}



		/// <summary>
		/// This method will be called on a background thread by the main game. All AI have a time limit of
		/// 10 seconds to complete their computations; if your AI takes longer than the time limit, the
		/// thread will be cancelled and any orders you have submitted will be committed.
		///
		/// This method receives its own copy of the SOGGame object; you can feel free to change values in 
		/// here without fear of breaking anything. The only part which needs to remain intact at the
		/// end of your processing is the SOEEmpire.Orders list; this list contains all of the orders
		/// your AI registered.
		///
		/// In addition, the contents of the SOGGame will only contain information which is available to your
		/// AI at the time of this turn processing. For example, if you iterate over the star systems in the
		/// galaxy ( foreach (SOMSystem system in game.Map.Systems) ), and then iterate over all of the
		/// planets in each system ( foreach(SOMPlanet planet in system.Planets) ), only the systems which
		/// you have explored will actually contain planets.
		/// </summary>
		/// <param name="game">The game in which your AI is playing.</param>
		/// <param name="aiEmpire">The empire which your AI represents.</param>

		public void ProcessTurn(SOGGame game, SOEEmpire aiEmpire) {

			// This sample AI will do some basic galactic exploration
			PerformExploration(game, aiEmpire);

		}



		// **************************** End required methods for so2ai interface **************************** 



		/// <summary>
		/// This is a sample method for ordering scouts around for exploration. It finds all currently idling
		/// scouts, cross references to all unexplored and in range star systems, and disperses the scouts accordingly
		/// </summary>
		/// <param name="game">Game.</param>
		/// <param name="aiEmpire">Ai empire.</param>

		public void PerformExploration (SOGGame game, SOEEmpire aiEmpire) {
			string error;

			// run through all of my fleets, find all of the scouts in the fleet stationary in orbit
			List<SOMShip> availableScouts = new List<SOMShip>();
			SOMFleet[] myFleets = game.Map.AllFleetsOfEmpire (aiEmpire);

			foreach (SOMFleet fleet in myFleets) {
				if (fleet.orbitingSystem != null) {
					foreach (SOMShip ship in fleet.Ships) {
						if (ship.isScout) {
							availableScouts.Add (ship);
						}
					}
				}
			}

			// for all of the available scouts, find the closest, unexplored system
			List<SOMSystem> systemsWhichWeSendScoutsToAlready = new List<SOMSystem>();

			foreach (SOMSystem system in game.Map.Systems) {

				bool ignoreSystem = false;

				// Do I have any fleets already incoming to this system?
				foreach (SOMFleet fleet in myFleets) {
					if (fleet.toSystem != null && fleet.toSystem.Equals (system.uuid)) {
						ignoreSystem = true;
					}
				}

				if (ignoreSystem == true) {
					continue;
				}
					
				SOMShip closestScout = null;
				int minimumDistance = 9999999;

				foreach (SOMShip ship in availableScouts) {
					// Have I not explored this system and can my ships travel there?
					if (system.EmpireHasExplored (aiEmpire) == false &&
					    aiEmpire.CanShipTravelToSystem (ship, system)) {

						SOMSystem fromSystem = game.GetSystemReference (ship.Fleet ().orbitingSystem);
						int turnsToGetThere = fromSystem.TravelTimeToSystem (system, aiEmpire);

						if (turnsToGetThere < minimumDistance) {
							minimumDistance = turnsToGetThere;
							closestScout = ship;
						}

					}
				}

				if (closestScout != null) {
					// Register an order to send this ship to this system; in addition, make note that we got 
					// this system covered, and remove this scout from the available scouts
					SOCOrderShipsMove.RegisterOrder (aiEmpire, out error, closestScout.Fleet (), new List<object> { closestScout }, system);
					if (error == null) {
						availableScouts.Remove (closestScout);
						systemsWhichWeSendScoutsToAlready.Add (system);
					}
				}

			}

		}



	}
	
}
