[collections.arraylist] $content = (Get-Content .\index.html);
$content = $content.Replace('http://growbox01/', '');
$content = $content.Replace('http://growboxtest/', '');
$content = $content.Replace('http://192.168.178.197/', '');
$content.Insert(0, "const char HTML_INDEX[] PROGMEM = R""=====(");
$content.Add(")====="";");
$content | Set-Content .\index.h
