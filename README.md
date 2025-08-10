# FolderCompareCLI


# Running Guide

Folder Compare Cli is similar to my Folder Compare but with out the overhead of using a GUI as such can run in .net 6 and is platform independent.

## Arguments
<p>Arguments required expected args: {Source}  {Destination} {flags}</p>
<p>Source : source path for comparison</p>
<p>Destination: destination path for comparison</p>
<br>
<p> checking hash of files is off by default but can be enabled with optional flags</p>
<p> optional flag -hash enables hash check using md5  off by default</p>
<p> optional flag -hash256 enables hash check using sha256 off by default</p>



## Running
```
dotnet FolderCompareCLI.dll /home/user/folderOne  "/run/media/Back up" 
```

it will build a data model of the differences
![image](https://raw.githubusercontent.com/mkbmain/FolderCompareCLI/main/Images/Building.png)


it will then either report that there are no differences is which case every things is in sync 

or you will a screen similar to the following

![image](https://raw.githubusercontent.com/mkbmain/FolderCompareCLI/main/Images/Options.png)

please note on enter option we can enter from 0-6 here (this will adjust the numbered displayed based on your terminal size).
We can also enter N for Next page or e for exit (as the results are paginated).

If we select option 1 we will get a detailed screen given us a option of action to take to rectify this difference.
![image](https://raw.githubusercontent.com/mkbmain/FolderCompareCLI/main/Images/DetailOptions.png)

from here its all self explanatory :) 






## Enjoy


of course goes with out saying this has worked for my purposes feel free to edit it for yours.


### Legal
This is released under a free to use free to modify (do what you want with the code and images) license. I take no responsibility for damages or loss\corruption of data.