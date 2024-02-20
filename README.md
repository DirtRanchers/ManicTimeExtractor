# Purpose
Extracts time from a Manic Time (time-tracking) database and displays it in a grid.
I use it to display time stats in a way that makes it easy to transfer time entries to my company's timesheet.

Manic Time can be downloaded from https://www.manictime.com

This tool currently works with ManicTime 2023.3.3.0.  It likely works with many older and newer versions, but I haven't tested it.
Find older releases of ManicTime @ https://www.manictime.com/Releases/#.

In addition, this is tested only against the Windows single-user version; it has never been tested against the Server version nor has it been compiled to run against the Mac/Linux/Android versions.   

# How it Works
This software totals the Manic Time logged data, per day, per tag.

Note:  Time "slices" in Manic Time CAN be tagged with a single tag, or with multiple tags (separated by commas.)  Multiple tags can be used on however you wish in Manic Time, but Manic Time uses only the first tag assigned to a time slice to determine which overall "category" the slice belongs to.   This tool follows the same philosophy.  Only the first tag in each list is evaluated, and the final display is grouped by--and displays--only the first tag.
