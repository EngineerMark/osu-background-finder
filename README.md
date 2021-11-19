# osu-background-finder

## Intro
A simple comparison tool that attempts to find osu! beatmaps based on the image input.

Does not deal with crops etc, it simply picks 100 random points each image file and compares those pixel colors

Runs multithreaded.
26k maps takes a couple of minutes to process.


The code is bad, I know. I was bored and saw AzerFrost's tweet.


Only works on Windows because Bitmap usage.

## How to use

Build the project and run it.

First expected input is path to the image you search with (including the file itself + extension)

Second input is the directory for your osu beatmaps (osu dir\Songs), no trailing slash

Press enter

And wait.
