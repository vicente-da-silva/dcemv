for /f "delims=" %%# in ('powershell get-date -format "{ddMMyyyy}"') do @set _date=%%#
git archive --format=zip --output="../DC_EMV_BACKUP/DC_EMV_GH_%_date%.zip" HEAD