﻿using System.Text.RegularExpressions;
using Server.Models.DQL;
using Server.Parser.Actions;
using Server.Server.Cache;
using Server.Server.Requests;
using Server.Server.Requests.Controllers.Parser;

namespace Server.Parser.Commands;

internal class Use : BaseDbAction
{
    private readonly UseModel _model;

    public Use(Match match) => _model = UseModel.FromMatch(match);

    public override void PerformAction(Guid session)
    {
        CacheStorage.Set(session, _model.DatabaseName);
    }
}