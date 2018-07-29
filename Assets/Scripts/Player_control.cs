using System;

public static class Player_control
{
    private static String[] playersName = new String[4];
    public static int numPlayers;
    public static String playerLocalName;

    public static String[] getplayersName()
    {
        return playersName;
    }

    public static void addPlayerName(String player, int numPlayer)
    {
        playersName[numPlayer] = player;
    }
}