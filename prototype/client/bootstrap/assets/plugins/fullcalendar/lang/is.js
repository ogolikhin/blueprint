!function(a){"function"==typeof define&&define.amd?define(["jquery","moment"],a):a(jQuery,moment)}(function(a,b){function c(a){return a%100===11?!0:a%10===1?!1:!0}function d(a,b,d,e){var f=a+" ";switch(d){case"s":return b||e?"nokkrar sekúndur":"nokkrum sekúndum";case"m":return b?"mínúta":"mínútu";case"mm":return c(a)?f+(b||e?"mínútur":"mínútum"):b?f+"mínúta":f+"mínútu";case"hh":return c(a)?f+(b||e?"klukkustundir":"klukkustundum"):f+"klukkustund";case"d":return b?"dagur":e?"dag":"degi";case"dd":return c(a)?b?f+"dagar":f+(e?"daga":"dögum"):b?f+"dagur":f+(e?"dag":"degi");case"M":return b?"mánuður":e?"mánuð":"mánuði";case"MM":return c(a)?b?f+"mánuðir":f+(e?"mánuði":"mánuðum"):b?f+"mánuður":f+(e?"mánuð":"mánuði");case"y":return b||e?"ár":"ári";case"yy":return c(a)?f+(b||e?"ár":"árum"):f+(b||e?"ár":"ári")}}(b.defineLocale||b.lang).call(b,"is",{months:"janúar_febrúar_mars_apríl_maí_júní_júlí_ágúst_september_október_nóvember_desember".split("_"),monthsShort:"jan_feb_mar_apr_maí_jún_júl_ágú_sep_okt_nóv_des".split("_"),weekdays:"sunnudagur_mánudagur_þriðjudagur_miðvikudagur_fimmtudagur_föstudagur_laugardagur".split("_"),weekdaysShort:"sun_mán_þri_mið_fim_fös_lau".split("_"),weekdaysMin:"Su_Má_Þr_Mi_Fi_Fö_La".split("_"),longDateFormat:{LT:"H:mm",LTS:"LT:ss",L:"DD/MM/YYYY",LL:"D. MMMM YYYY",LLL:"D. MMMM YYYY [kl.] LT",LLLL:"dddd, D. MMMM YYYY [kl.] LT"},calendar:{sameDay:"[í dag kl.] LT",nextDay:"[á morgun kl.] LT",nextWeek:"dddd [kl.] LT",lastDay:"[í gær kl.] LT",lastWeek:"[síðasta] dddd [kl.] LT",sameElse:"L"},relativeTime:{future:"eftir %s",past:"fyrir %s síðan",s:d,m:d,mm:d,h:"klukkustund",hh:d,d:d,dd:d,M:d,MM:d,y:d,yy:d},ordinalParse:/\d{1,2}\./,ordinal:"%d.",week:{dow:1,doy:4}}),a.fullCalendar.datepickerLang("is","is",{closeText:"Loka",prevText:"&#x3C; Fyrri",nextText:"Næsti &#x3E;",currentText:"Í dag",monthNames:["Janúar","Febrúar","Mars","Apríl","Maí","Júní","Júlí","Ágúst","September","Október","Nóvember","Desember"],monthNamesShort:["Jan","Feb","Mar","Apr","Maí","Jún","Júl","Ágú","Sep","Okt","Nóv","Des"],dayNames:["Sunnudagur","Mánudagur","Þriðjudagur","Miðvikudagur","Fimmtudagur","Föstudagur","Laugardagur"],dayNamesShort:["Sun","Mán","Þri","Mið","Fim","Fös","Lau"],dayNamesMin:["Su","Má","Þr","Mi","Fi","Fö","La"],weekHeader:"Vika",dateFormat:"dd.mm.yy",firstDay:0,isRTL:!1,showMonthAfterYear:!1,yearSuffix:""}),a.fullCalendar.lang("is",{buttonText:{month:"Mánuður",week:"Vika",day:"Dagur",list:"Dagskrá"},allDayHtml:"Allan<br/>daginn",eventLimitText:"meira"})});