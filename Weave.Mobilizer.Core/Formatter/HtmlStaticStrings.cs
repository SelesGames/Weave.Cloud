
namespace Weave.Mobilizer.Core
{
    // color 1                 background-color: #DCE3DE;
    // good color                 background-color: #E8EBEA;

    internal static class HtmlStaticStrings
    {
        //            html { -ms-text-size-adjust:150%; }

        internal const string STYLE =
@"
            body {
                font-family: Segoe WP;
                line-height: 1.6;
                font-size: 18px;
                color: #222222;
                background-color: #E8EBEA;
                margin-left: 5px;
                margin-right: 5px;
                word-wrap: break-word;
            }
            
            h1 { 
                font-size: 38px; 
                font-family: Segoe WP Light; 
                line-height: 1.2; 
                margin-top: 8px; 
                margin-bottom: 28px; 
                color: black;
                word-wrap: break-word;
            }
            h2 { 
                font-size: 16px; 
                font-family: Segoe WP Semibold; 
                line-height: 1.3;
                color: #555555;
            }
            h3, h4, h5, h6, h7 { font-size: 1.0em; }
            
            img { 
                border: 0; 
                display: block; 
                margin-top: 24px;
                margin-bottom: 24px;
                margin-left: 0px;
                margin-right: 0px;
                width: expression(this.width > 282 ? 282 : true);
            }

            pre, code { overflow: scroll; }
            #story { 
                                clear: both; padding: 0 10px; overflow: hidden; margin-bottom: 40px; 
            }

            .bar {
                color: #555;
                font-family: 'Helvetica'; 
                font-size: 11pt;
                                    margin: 0 -20px;
                    padding: 10px 0;
                            }            
            .top { border-bottom: 2px solid #000; }
            
                        
            #story div {
                margin: 1em 0;
            }
            
            .bottom {
                border-top: 2px solid #000;
                color: #555;
            }

            .bar a { color: #444; }
            
            blockquote {
                border-top: 1px solid #bbb;
                border-bottom: 1px solid #bbb;
                margin: 1.5em 0;
                padding: 0.5em 0;
            }
            blockquote.short { font-style: italic; }
            
            pre {
                white-space: pre-wrap;
            }
            
            ul.bodytext, ol.bodytext {
            	list-style: none;
            	margin-left: 0;
            	padding-left: 0em;
            }
        ";

        internal const string SCRIPT =
@"
            function test()
            {
                ;
            }

            function findFirstDescendant(parent, tagname)
            {
               var descendants = parent.getElementsByTagName(tagname);
               if ( descendants.length )
                  return descendants[0];
               return null;
            }

            function colorFontsAndBackground(accentColor, foreground, background)
            {
                //document.bgColor = background;
                //document.body.style.color = foreground;
                var h = findFirstDescendant(document.body, ""h1"");
                h.style.color = accentColor;
                document.linkColor = accentColor;
            }
        ";

        internal const string BRANDING = 
@"
        <p style=""margin-left:10px; font-weight:800; margin-bottom:24px;"">Powered by Weave</p><div/>";
    }
}

//               document.body.style.display=\"none";

//
//for (x = 0; x < document.links.length; x++)
//  document.links[x].style.color = accentColor;

