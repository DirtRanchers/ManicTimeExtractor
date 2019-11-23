# Purpose
Extracts time from a Manic Time (time-tracking) database and displays it in a grid.
I use it to display time stats in a way that makes it easy to transfer time entries to my company's timesheet.

Manic Time can be downloaded from https://www.manictime.com

This works with ManicTime 3.7.4 and earlier.  It likely also works with 3.8.* but I haven't tested it.
It does NOT work with version 4.0+, because the software switched DB providers and structure.
(Releases older than current can be downloaded here: https://www.manictime.com/Releases/#)

Tested only against the Windows desktop version. 

# How it Works
This software totals the Manic Time logged data, per day, per tag.

Note:  Time entries in Manic Time CAN be tagged with a single tag, or with multiple tags (separated by commas.)  Multiple tags can be used however you wish in Manic Time, but Manic Time uses only the first in a comma-delimited list to determine the main "category" and choose a display color.   This tool follows the same philosophy.  Only the first tag in case list is evaluated.
