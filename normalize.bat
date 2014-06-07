ECHO OFF
REM angol
REM CorpusNormalizer -traincount=200000 -evalcount=20000 -inputfile="H:\Documents\BME_VIK\PhD\szovegek\europarl\en-de\Europarl.de-en.en" -trainfile="H:\temp\monoling-predtype\en-de\Europarl.en-de.en.train" -evalfile="H:\temp\monoling-predtype\en-de\Europarl.en-de.en.eval"

REM német
REM CorpusNormalizer -traincount=200000 -evalcount=20000 -inputfile="H:\Documents\BME_VIK\PhD\szovegek\europarl\en-de\Europarl.de-en.de" -trainfile="H:\temp\monoling-predtype\en-de\Europarl.en-de.de.train" -evalfile="H:\temp\monoling-predtype\en-de\Europarl.en-de.de.eval"

REM angol
REM CorpusNormalizer -traincount=200000 -evalcount=20000 -inputfile="H:\Documents\BME_VIK\PhD\szovegek\europarl\en-fr\Europarl.en-fr.en" -trainfile="H:\temp\monoling-predtype\en-fr\Europarl.en-fr.en.train" -evalfile="H:\temp\monoling-predtype\en-fr\Europarl.en-fr.en.eval"

REM francia
REM CorpusNormalizer -traincount=200000 -evalcount=20000 -inputfile="H:\Documents\BME_VIK\PhD\szovegek\europarl\en-fr\Europarl.en-fr.fr" -trainfile="H:\temp\monoling-predtype\en-fr\Europarl.en-fr.fr.train" -evalfile="H:\temp\monoling-predtype\en-fr\Europarl.en-fr.fr.eval"

REM angol
REM CorpusNormalizer -traincount=200000 -evalcount=20000 -inputfile="H:\Documents\BME_VIK\PhD\szovegek\europarl\en-es\Europarl.en-es.en" -trainfile="H:\temp\monoling-predtype\en-es\Europarl.en-es.en.train" -evalfile="H:\temp\monoling-predtype\en-es\Europarl.en-es.en.eval"

REM spanyol
REM CorpusNormalizer -traincount=200000 -evalcount=20000 -inputfile="H:\Documents\BME_VIK\PhD\szovegek\europarl\en-es\Europarl.en-es.es" -trainfile="H:\temp\monoling-predtype\en-es\Europarl.en-es.es.train" -evalfile="H:\temp\monoling-predtype\en-es\Europarl.en-es.es.eval"

REM angol
CorpusNormalizer -traincount=200000 -evalcount=20000 -inputfile="H:\Documents\BME_VIK\PhD\szovegek\europarl\en-hu\Europarl.en-hu.en" -trainfile="H:\temp\predtype\en-hu\Europarl.en-hu.en.train" -evalfile="H:\temp\predtype\en-hu\Europarl.en-hu.en.eval"

REM magyar
CorpusNormalizer -traincount=200000 -evalcount=20000 -inputfile="H:\Documents\BME_VIK\PhD\szovegek\europarl\en-hu\Europarl.en-hu.hu" -trainfile="H:\temp\predtype\en-hu\Europarl.en-hu.hu.train" -evalfile="H:\temp\predtype\en-hu\Europarl.en-hu.hu.eval"