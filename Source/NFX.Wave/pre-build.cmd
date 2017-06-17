set SOLUTION_DIR=%1
set PROJECT_DIR=%~dp0

buildinfo > "%PROJECT_DIR%BUILD_INFO.txt"
ntc "%PROJECT_DIR%Templatization\StockContent\*.htm" -r -ext ".auto.cs" -src -c "NFX.Templatization.NHTCompiler, NFX"
ntc "%PROJECT_DIR%Templatization\StockContent\Embedded\script\ljs\*.ntc.js" -ext ".js" -replace ".ntc.js" -dest "%PROJECT_DIR%Templatization\StockContent\Embedded\script" -src -c "NFX.Templatization.TextJSTemplateCompiler, NFX" -o dom-gen="cmp{pretty=1}"

where /q java
if ERRORLEVEL 1 goto NO_JAVA

java -jar "%SOLUTION_DIR%lib\closure-compiler\compiler.jar" ^
   --js "%PROJECT_DIR%Templatization\StockContent\Embedded\script\wv.js" ^
        "%PROJECT_DIR%Templatization\StockContent\Embedded\script\wv.gui.js" ^
        "%PROJECT_DIR%Templatization\StockContent\Embedded\script\wv.chart.svg.js" ^
        "%PROJECT_DIR%Templatization\StockContent\Embedded\script\wv.braintree.js" ^
        "%PROJECT_DIR%Templatization\StockContent\Embedded\script\wv.stripe.js" ^
   --js_output_file "%PROJECT_DIR%Templatization\StockContent\Embedded\script\wv.all.min.js" ^
   --compilation_level SIMPLE_OPTIMIZATIONS ^
   --language_in ECMASCRIPT5_STRICT > "%PROJECT_DIR%Templatization\StockContent\Embedded\script\CLOSURE_ERROR_OUT.txt" 2>&1

:NO_JAVA
exit 0