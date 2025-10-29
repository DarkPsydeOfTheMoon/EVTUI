using System;
using System.Collections.Generic;

using ReactiveUI;

using static EVTUI.ViewModels.FieldUtils;

namespace EVTUI.ViewModels.TimelineCommands;

public class Fd__ : Generic
{
    public Fd__(DataManager config, CommandPointer cmd) : base(config, cmd)
    {
        this.LongName = "Fade";

        this.FadeMode = new StringSelectionField("Fade Mode", this.Editable, this.FadeModes.Backward[this.CommandData.FadeMode], this.FadeModes.Keys);
        this.WhenAnyValue(_ => _.FadeMode.Choice).Subscribe(_ => this.CommandData.FadeMode = this.FadeModes.Forward[this.FadeMode.Choice]);
        this.FadeType = new StringSelectionField("Fade Type", this.Editable, this.FadeTypes.Backward[this.CommandData.FadeType], this.FadeTypes.Keys);
        this.WhenAnyValue(_ => _.FadeType.Choice).Subscribe(_ => this.CommandData.FadeType = this.FadeTypes.Forward[this.FadeType.Choice]);
        this.UnkBool = new BoolChoiceField("Unknown", this.Editable, this.CommandData.UnkBool != 0);
        this.WhenAnyValue(_ => _.UnkBool.Value).Subscribe(_ => this.CommandData.UnkBool = Convert.ToByte(this.UnkBool.Value));
    }

    public StringSelectionField FadeMode { get; set; }
    public StringSelectionField FadeType { get; set; }
    public BoolChoiceField      UnkBool  { get; set; }

    public BiDict<string, byte> FadeModes = new BiDict<string, byte>
    (
        new Dictionary<string, byte>
        {
            {"None",     0},
            {"Fade-In",  1},
            {"Fade-Out", 2},
        }
    );

    // some strings fully copied from DC's EVTFADEOUTTABLE merging readme
    // ...until i realized the enums don't match lmfao. but i'm leaving them in for now
    public BiDict<string, byte> FadeTypes = new BiDict<string, byte>
    (
        new Dictionary<string, byte>
        {
            {"Default",                  0},  // confirmed
            //{"Default",                  1},
            {"Unattested #1",            1},
            //{"Crowd Walking #1",         2},
            {"Unattested #2",            2},
            //{"PT Joker Jumps Out #1",    3},
            {"Unattested #3",            3},
            //{"PT Joker Jumps Out #2",    4},
            {"Unattested #4",            4},
            {"Screen Wipe #1",           5},  // confirmed
            //{"Crowd Walking #2",         6},
            {"Unattested #5",            6},
            //{"Crowd Walking #3",         7},
            {"Unattested #6",            7},
            {"Train Transition",         8},  // confirmed
            //{"Crowd Walking #4",         9},
            {"Unattested #7",            9},
            //{"Crowd Walking #5",        10},
            {"Unattested #8",           10},
            //{"Umbrellas",               11},
            {"Unattested #9",           11},
            //{"Cloth Removal",           12},
            {"Unattested #10",          12},
            //{"Screen Freeze #1",        13},
            {"Unattested #11",          13},
            {"Metaverse",               14},  // confirmed
            //{"Take Your Time (PT) #1",  15},
            //{"Flashback",               16},
            {"Unattested #12",          15},
            {"Take Your Time #1",       16},  // confirmed
            {"Flashback",               17},  // confirmed
            {"Velvet Room",             18},  // pretty sure lol
            {"Screen Wipe #2",          19},  // confirmed
            //{"Morgana Bus",             20},
            {"Unattested #13",          20},
            //{"School Trip Airplane",    21},
            //{"Rapid Flash-Forward",     22},
            {"Rapid Flash-Forward",     21},  // confirmed
            {"Unattested #14",          22},
            {"Metaverse + Transparent", 23},  // confirmed
            //{"Entering Metaverse #2",   24},
            {"Unattested #15",          24},
            //{"Metaverse, Then Black",   25},
            {"Unattested #16",          25},
            //{"Black, Then Metaverse",   26},
            {"Unattested #17",          26},
            //{"Crowd Walking #6",        27},
            //{"Take Your Time (PT) #2",  28},
            {"Take Your Time #2",       27},  // confirmed
            //{"Crowd Walking #6",        28},
            {"Unattested #18",          28},
            //{"Crowd Walking (Beach)",   29},
            {"Unattested #19",          29},
            //{"Screen Freeze #2",        30},
            {"Unattested #20",          30},
        }
    );
}
