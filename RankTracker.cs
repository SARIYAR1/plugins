using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using System.Collections.ObjectModel;
//using System.Timers;
using CodeHatch.Engine.Networking;
//using CodeHatch.Engine.Core.Networking;
//using CodeHatch.Thrones.SocialSystem;
using CodeHatch.Common;
//using CodeHatch.Permissions;
using Oxide.Core;
using CodeHatch.Networking.Events;
using CodeHatch.Networking.Events.Entities;
using CodeHatch.Networking.Events.Entities.Players;
using CodeHatch.Networking.Events.Players;
//using CodeHatch.ItemContainer;
using CodeHatch.UserInterface.Dialogues;
//using CodeHatch.Inventory.Blueprints.Components;

namespace Oxide.Plugins
{
    [Info("Rank Tracker", "Scorpyon", "1.0.3")]
    public class RankTracker : ReignOfKingsPlugin
    {
		private int xpRewardForPve = 5; // (Maximum xp - given in random increments 1-5)
		
        void Log(string msg) => Puts($"{Title} : {msg}");
		private Dictionary<string,int> rankList = new Dictionary<string,int>();
		private System.Random random = new System.Random();
		
		
#region User Commands
        
        // View my rank
        [ChatCommand("rank")]
        private void ShowMyRank(Player player, string cmd)
        {
            ShowMyPlayerRank(player, cmd);
        }
		
        // View the server player ranks
        [ChatCommand("rankings")]
        private void ShowPlayerRanks(Player player, string cmd)
        {
            ShowPlayerRanksForServer(player, cmd);
        }
		
		// Give a player XP (Admin)
        [ChatCommand("giverankxp")]
        private void GivePlayerXp(Player player, string cmd, string[] input)
        {
            GivePlayerSomeXp(player, cmd, input);
        }
		
		// Set a player's XP (Admin)
        [ChatCommand("setrankxp")]
        private void SetPlayerXp(Player player, string cmd, string[] input)
        {
            SetTheRankXPForAPlayer(player, cmd, input);
        }
		
		

#endregion


#region Save and Load Data Methods

        // SAVE DATA ===============================================================================================
        private void LoadRankData()
        {
            rankList = Interface.GetMod().DataFileSystem.ReadObject<Dictionary<string,int>>("SavedRankList");
        }

        private void SaveRankData()
        {
            Interface.GetMod().DataFileSystem.WriteObject("SavedRankList", rankList);
        }
		
		private void OnPlayerConnected(Player player)
		{
			CheckRankExists(player);

			//Set player's rank
			SetPlayerRank(player);
			
			// Save the trade data
            SaveRankData();
		}
		
		
		private void CheckRankExists(Player player)
		{
			//Check if the player has a wallet yet
			if(rankList.Count < 1) rankList.Add(player.Name.ToLower(),0);
			if(!rankList.ContainsKey(player.Name.ToLower()))
			{
				rankList.Add(player.Name.ToLower(),0);
			}
		}
		
        void Loaded()
        {
            LoadRankData();
            
            // Save the trade data
            SaveRankData();
        }
        // ===========================================================================================================
		
#endregion

#region Private Methods

		private string GetRank(int xp)
        {
			var rank = "[003333]Civilian[FFFFFF]";
			
			if(xp > 1000000) return "[FB0000]High Commander[FFFFFF]";
			if(xp > 500000) return "[FE6542]Commander[FFFFFF]";
			if(xp > 200000) return "[FCCE7D]Chancellor[FFFFFF]";
			if(xp > 100000) return "[E0FC7D]Baron[FFFFFF]";
			if(xp > 50000) return "[8FFC7D]Minor Baron[FFFFFF]";
			if(xp > 20000) return "[A0F8E2]Lord[FFFFFF]";
			if(xp > 10000) return "[A0D8F8]Minor Lord[FFFFFF]";
			if(xp > 5000) return "[7688F7]Knight[FFFFFF]";
			if(xp > 2000) return "[C8C6FA]Squire[FFFFFF]";
			if(xp > 1000) return "[E9C46D]Knave[FFFFFF]";
			if(xp > 500) return "[9A6BA0]Manservant[FFFFFF]";
			if(xp > 250) return "[6C857C]Servant[FFFFFF]";
			if(xp > 100) return "[AEB5B2]Serf[FFFFFF]";
			
			return rank;
        }

        private void ShowMyPlayerRank(Player player, string cmd)
        {
            if (!rankList.ContainsKey(player.Name.ToLower()))
            {
                PrintToChat(player, "You will need to log off and back on again for your rank to come into effect.");
                return;
            }
            var myXP = rankList[player.Name.ToLower()];
            var myRank = GetRank(myXP);

            //var rank = "[003333]Civilian[FFFFFF]";
            //if(xp > 1000000) return "[FB0000]High Commander[FFFFFF]";
            //if(xp > 500000) return "[FE6542]Commander[FFFFFF]";
            //if(xp > 200000) return "[FCCE7D]Chancellor[FFFFFF]";
            //if(xp > 100000) return "[E0FC7D]Baron[FFFFFF]";
            //if(xp > 50000) return "[8FFC7D]Minor Baron[FFFFFF]";
            //if(xp > 20000) return "[A0F8E2]Lord[FFFFFF]";
            //if(xp > 10000) return "[A0D8F8]Minor Lord[FFFFFF]";
            //if(xp > 5000) return "[7688F7]Knight[FFFFFF]";
            //if(xp > 2000) return "[C8C6FA]Squire[FFFFFF]";
            //if(xp > 1000) return "[E9C46D]Knave[FFFFFF]";
            //if(xp > 500) return "[9A6BA0]Manservant[FFFFFF]";
            //if(xp > 250) return "[6C857C]Servant[FFFFFF]";
            //if(xp > 100) return "[AEB5B2]Serf[FFFFFF]";

            var itemText = "\n" +
                           "[FB0000]High Commander[FFFFFF]  - 1,000,000 XP\n" +
                           "[FE6542]Commander[FFFFFF]  -  500,000 XP\n" +
                           "[FCCE7D]Chancellor[FFFFFF]  -  250,000 XP\n" +
                           "[E0FC7D]Baron[FFFFFF]  -  100,000\n" +
                           "[8FFC7D]Minor Baron[FFFFFF]  -  50,000 XP\n" +
                           "[A0D8F8]Minor Lord[FFFFFF]  -  20,000 XP\n" +
                           "[A0D8F8]Minor Lord[FFFFFF]  -  10,000 XP\n" +
                           "[7688F7]Knight[FFFFFF]  -  5,000 XP\n" +
                           "[C8C6FA]Squire[FFFFFF]  -  2,000 XP\n" +
                           "[E9C46D]Knave[FFFFFF]  -  1,000 XP\n" +
                           "[9A6BA0]Manservant[FFFFFF]  -  500 XP\n" +
                           "[6C857C]Servant[FFFFFF]  -  250 XP\n" +
                           "[AEB5B2]Serf[FFFFFF]  -  100 XP\n" +
                           "[003333]Civilian[FFFFFF]\n" +
                           "\n" +
                           "\n" +
                           "My Current Rank  -  [00FF00]" + myRank + "\n" + 
                           "My Current XP  -  [FFFF00]" + myXP.ToString();

            player.ShowPopup("My Rank", itemText, "Exit", (selection, dialogue, data) => DoNothing(player, selection, dialogue, data));   
        }

        
        private void SetTheRankXPForAPlayer(Player player, string cmd, string[] input)
        {
            if (!player.HasPermission("admin"))
            {
                PrintToChat(player, "Only admins can use this command.");
                return;
            }
			
			if(input.Length < 2)
			{
				PrintToChat(player, "Please enter a player name and a valid amount of XP to give.");
                return;
			}
			
			var playerName = input[0];
			var target = Server.GetPlayerByName(playerName);
			
			if(target == null)
			{	
				PrintToChat(player, "That player does not seem to be online.");
                return;
			}
			
			int amount;
			if(Int32.TryParse(input[1], out amount) == false)
			{
				PrintToChat(player, "That was not a recognised amount!");
                return;
			}
			
			rankList[playerName.ToLower()] = amount;
            SetPlayerRank(player);
			PrintToChat(player, playerName + " has had their rank XP set to " + GetRank(amount) + "!");
        }

		private void GivePlayerSomeXp(Player player, string cmd, string[] input)
		{
			if (!player.HasPermission("admin"))
            {
                PrintToChat(player, "Only admins can use this command.");
                return;
            }
			
			if(input.Length < 2)
			{
				PrintToChat(player, "Please enter a player name and a valid amount of XP to give.");
                return;
			}
			
			var playerName = input[0];
			var target = Server.GetPlayerByName(playerName);
			
			if(target == null)
			{	
				PrintToChat(player, "That player does not seem to be online.");
                return;
			}
			
			PrintToChat(input[1].ToString());
			int amount;
			if(Int32.TryParse(input[1], out amount) == false)
			{
				PrintToChat(player, "That was not a recognised amount!");
                return;
			}
			
			AddRankXp(target, amount);
			PrintToChat(player, playerName + " has been promoted to " + GetRank(amount) + "!");
		}

		
		private void OnEntityHealthChange(EntityDamageEvent damageEvent) 
		{
			if (!damageEvent.Entity.IsPlayer)
			{
				var victim = damageEvent.Entity;
				Health h = victim.TryGet<Health>();
				if(h.ToString().Contains("Plague Villager")) return;
				if (!h.IsDead) return;
				
				var hunter = damageEvent.Damage.DamageSource.Owner;
				
				// Give the rewards to the player
				var xp = random.Next(1,xpRewardForPve);
				
				// Special bonuses
				if(h.ToString().Contains("Werewolf")) xp = xp + 10;
				if(h.ToString().Contains("Bear")) xp = xp + 5;
				if(h.ToString().Contains("Wolf")) xp = xp + 3;

				// Notify everyone
				PrintToChat(hunter, "[FFFFFF]You learned from your hunting and gained some experience! [FFFFFF]([00FF00]+" + xp.ToString() + "[FFFFFF])");
				AddRankXp(hunter,xp);
				
				SaveRankData();
			}
		}
		
		
		private void AddRankXp(Player player, int amount)
		{	
			CheckRankExists(player);
		    var currentRank = GetRank(rankList[player.Name.ToLower()]);
			var currentXP = rankList[player.Name.ToLower()];
			currentXP = currentXP + amount;
			if(currentXP > 21000000) currentXP = 21000000;
			rankList[player.Name.ToLower()] = currentXP;
			
			SetPlayerRank(player);
            var newRank = GetRank(rankList[player.Name.ToLower()]);
		    if (newRank != currentRank)
		    {
		        PrintToChat(player, "[FF0000]Rank Master[FFFFFF] : Congratulations! You have acquired a new rank!");
		    }

			SaveRankData();
		}
		
		private void SetPlayerRank(Player player)
		{
			CheckRankExists(player);
			var playerRank = Capitalise(GetRank(rankList[player.Name.ToLower()]));
			player.DisplayNameFormat = "[FFFFFF]([FFFFFF]" + playerRank + "[FFFFFF]) %name%";
		    SaveRankData();
		}
		
		private void ShowPlayerRanksForServer(Player player, string cmd)
        {
			var singlePage = false;
			var itemText = "";
			var itemsPerPage = 30;
			var currentItemCount = 0;
			
             // Are there any ranks yet
            if(rankList.Count == 0)
            {
                PrintToChat(player, "[FF0000]Rank Master[FFFFFF] : No-one has acquired any ranks yet.");
                return;
            }

            // Remove Server from rank list
		    if (rankList.ContainsKey("server"))
		    {
		        rankList.Remove("server");
		    }

			// Check number of players with a rank
            if(itemsPerPage > rankList.Count) 
			{
				singlePage = true;
				itemsPerPage = rankList.Count;
			}

		    var finalRankList = new Collection<string[]>();
            var isInserted = false;
            foreach (var item in rankList)
            {
                var finalRankListCount = finalRankList.Count;
                for (var itemCount = 0; itemCount < finalRankListCount; itemCount++)
                {
                    var xp = item.Value;
                    if (xp > Int32.Parse(finalRankList[itemCount][1]))
                    {
                        finalRankList.Insert(itemCount, new string[2] {item.Key, item.Value.ToString()});
                        isInserted = true;
                        break;
                    }
                }
                if (!isInserted)
                {
                    finalRankList.Add(new string[2] { item.Key, item.Value.ToString()});
                }
                isInserted = false;
            }

			var i=0;
			foreach(var person in finalRankList)
            {
				var message = "[FFFFFF]" + Capitalise(person[0]) + " - (" + Capitalise(GetRank(Int32.Parse(person[1]))) + ")\n";
                itemText = itemText + message;
				i++;
				if(i >= itemsPerPage + currentItemCount) break;
            }
			
			if(singlePage) 
			{
				player.ShowPopup("Duke's Castle Rankings", itemText, "Exit", (selection, dialogue, data) => DoNothing(player, selection, dialogue, data));
				return;
			}
			
            //Display the Popup with the price
			player.ShowConfirmPopup("Duke's Castle Rankings", itemText, "Next Page", "Exit", (selection, dialogue, data) => ContinueWithRankList(player, selection, dialogue, data, itemsPerPage, itemsPerPage));
        }
		
		
		private void ContinueWithRankList(Player player, Options selection, Dialogue dialogue, object contextData,int itemsPerPage, int currentItemCount)
		{
            if (selection != Options.Yes)
            {
                //Leave
                return;
            }
			
			if((currentItemCount + itemsPerPage) > rankList.Count)
			{
				itemsPerPage = rankList.Count - currentItemCount;
			}
            
			var itemText = "";
			var i = currentItemCount;
			foreach(var person in rankList)
            {
			    string message = "[FFFFFF]" + Capitalise(person.Key) + " - (" + Capitalise(GetRank(person.Value)) + ")\n";
                itemText = itemText + message;
				i++;
				if(i >= itemsPerPage + currentItemCount) break;
            }

            currentItemCount = currentItemCount + itemsPerPage;

            // Display the Next page
            if(currentItemCount < rankList.Count)
            {
                player.ShowConfirmPopup("Duke's Castle Rankings", itemText,  "Next Page", "Exit", (options, dialogue1, data) => ContinueWithRankList(player, options, dialogue1, data, itemsPerPage, currentItemCount));
            }
            else
            {
                PlayerExtensions.ShowPopup(player,"Duke's Castle Rankings", itemText, "Yes",  (newselection, dialogue2, data) => DoNothing(player, newselection, dialogue2, data));
            }
		}

		// Capitalise the Starting letters
		private string Capitalise(string word)
		{
			var finalText = "";
			finalText = Char.ToUpper(word[0]).ToString();
			var spaceFound = 0;
			for(var i=1; i<word.Length;i++)
			{
				if(word[i] == ' ')
				{
					spaceFound = i + 1;
				}
				if(i == spaceFound)
				{
					finalText = finalText + Char.ToUpper(word[i]).ToString();
				}
				else finalText = finalText + word[i].ToString();
			}
			return finalText;
		}
		
		private void DoNothing(Player player, Options selection, Dialogue dialogue, object contextData)
		{
			//Do nothing
		}
		
#endregion

    }
}
