# Prerequisite #
> Latest [SubCentral](http://code.google.com/p/subcentral) plugin

# Installation #
> Install MPE1 file

> During installation, the following popup will show:
![https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Main1.png](https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Main1.png)

> Press the Update login info button. The following popup will show:
![https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Login.png](https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Login.png)

> Enter your user and password. Press Save.
![https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Main2.png](https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Main2.png)

> You're all set!

# Configuration #

> Enable _`"Sratim.co.il"`_ and _`"SubsCenter.Org"`_ providers via `SubCentral` configuration. Prioritize.
> Enable Hebrew language. Prioritize.
> You can also create different groups for Movies and TV shows.

> Configuration I use (you can do the same, or not...):
| **General** | **Languages** |
|:------------|:--------------|
| ![https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/General.png](https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/General.png) | ![https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Languages.png](https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Languages.png) |
| **Folders** | **Advanced** |
| ![https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Folders.png](https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Folders.png) | ![https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Advanced.png](https://hebrewsubtitledownloader.googlecode.com/svn/wiki/configuration/Advanced.png) |


> ## Fine Tuning for SubsCenter.Org ##
> In case you find a series that the site supports, but the provider fails to find, edit the _**`SubsCenterOrg.xml`**_ in the installation folder according to this example:
> <br>
<blockquote><br>If <i><b>House M.D.</b></i> appears in your <code>MediaPortal</code> as <i><b>House</b></i>, and the plugin does not find a match since there are too many series names with "House" in it, you can add specific parameter in the  <i><code>SubsCenterOrg.xml</code></i>
<br> the line<i><b><code>&lt;series query="house"&gt;house md&lt;/series&gt;</code></b></i>
<br> means that the local series name <i><b>"house"</b></i> will be changed to <i><b>"house md"</b></i>.<br>
<br> You can find the correct value from the url of the series page in <code>SubsCenter.org</code> site. In this case it's:<br>
<br> <i><code>http://www.subscenter.org/he/subtitle/series/</code></i><i><b><code>house-md</code></b><code>/</code></i>
<br>
<br> Note: You can use " " instead of "-"