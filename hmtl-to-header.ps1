[collections.arraylist] $content = (Get-Content .\index.html);
$content = $content.Replace('http://growbox01/', '');
$content.Insert(0, "const char HTML_INDEX[] PROGMEM = R""=====(");
$content.Add(")====="";");
$content | Set-Content .\index.h
