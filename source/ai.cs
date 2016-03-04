using System.Collections.Generic;


namespace SampleAI {
	
	public class AI {


		// This method will be called on a background thread by the main game. All AI have a time limit of
		// 10 seconds to complete their computations; if your AI takes longer than the time limit, the
		// thread will be cancelled and any orders you have submitted will be committed.
		//
		// This method receives its own copy of the SOGGame object; you can feel free to change values in 
		// here without fear of breaking anything. The only part which needs to remain intact at the
		// end of your processing is the SOEEmpire.Orders list; this list contains all of the orders
		// your AI registered.
		//
		// In addition, the contents of the SOGGame will only contain information which is available to your
		// AI at the time of this turn processing. For example, if you iterate over the star systems in the
		// galaxy ( foreach (SOMSystem system in game.Map.Systems) ), and then iterate over all of the
		// planets in each system ( foreach(SOMPlanet planet in system.Planets) ), only the systems which
		// you have explored will actually contain planets.

		public void ProcessTurn(SOGGame game, SOEEmpire aiEmpire) {

			// This sample AI will do some basic galactic exploration
			PerformExploration(game, aiEmpire);

		}

		public void PerformExploration (SOGGame game, SOEEmpire aiEmpire) {
			string error;

			// run through all of my fleets, find all of the scouts in the fleet stationary in orbit
			List<SOMShip> availableScouts = new List<SOMShip>();

			foreach (SOMFleet fleet in game.Map.AllFleetsOfEmpire(aiEmpire)) {
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
