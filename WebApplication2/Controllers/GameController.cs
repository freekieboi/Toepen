﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASPToep.Models;
using Logic;
using Logic.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ASPToep.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        GameLogic gameLogic = new GameLogic(false);
        GameInstance gameModel = new GameInstance();

        [Route("Game/CreateGame")]
        public IActionResult CreateGame()
        {
            CreateGameViewModel createGameViewModel = new CreateGameViewModel();
            createGameViewModel.AllUsers = gameLogic.GetAllUsers();
            return View(createGameViewModel);
        }

        [Route("Game/GameRoom")]
        public IActionResult GameRoom(string Datastream,  string Limit)
        {
            string[] datastream = Datastream.Split(';');           
            foreach (var id in datastream)
            {
                if (Int32.TryParse(id, out int num))
                {
                    gameModel.AddPlayer(gameLogic.GetPlayerById(num));
                }
            }
            gameModel.Limit = Int32.TryParse(Limit, out int numb) ? Convert.ToInt32(Limit) : 20;
            return View(new GameViewModel(gameModel));
        }

        [HttpPost]
        public JsonResult StartGame(string dataStream, string limit)
        {
            return Json(new { newUrl = Url.Action("GameRoom", "Game", new { DataStream = dataStream, Limit = limit }) });
        }

        // GAME BUTTON EVENTS
        [HttpPost]
        public ActionResult PlayerWonRound(string winPlayer, string json, string roundCount)
        {
            List<Player> tempPlayerList = gameLogic.GetPlayerList(json);
            gameModel.playerList = tempPlayerList;
            gameModel.Round = Convert.ToInt32(roundCount) + 1;

            foreach (var player in tempPlayerList)
            {
                if (player.Id != Convert.ToInt32(winPlayer))
                    player.CalculateScore();
            }

            foreach (var player in gameModel.playerList)
            {
                player.ResetRound();
            }
            return PartialView("Razorpages/GameContainer", gameModel);
        }

        [HttpPost]
        public IActionResult PlayerKnocks(string knockPlayer, string json, string roundCount)
        {
            List<Player> tempPlayerList = gameLogic.GetPlayerList(json);
            gameModel.playerList = tempPlayerList;
            gameModel.Round = Convert.ToInt32(roundCount);

            tempPlayerList.First(i => i.Id == Convert.ToInt32(knockPlayer)).Knocked++;
            return PartialView("Razorpages/GameContainer", gameModel);
        }
    }
}