<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:s="http://schemas.microsoft.com/sqlserver/2004/07/showplan"
    exclude-result-prefixes="msxsl s xsl">
<!--<xsl:output method="html" indent="no"
		  doctype-system="http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"
		  doctype-public="-//W3C//DTD XHTML 1.0 Transitional//EN" />-->
      <xsl:include href="qp.xslt" />

  <xsl:template match="/">
    <div>
      <link rel="stylesheet" type="text/css" href="/Content/qp.css" />
      <script src="/Scripts/qp.js" type="text/javascript"></script>
      <script type="text/javascript">$(document).ready( function() { QP.drawLines(); });</script>
      <div>
        <xsl:apply-templates select="s:ShowPlanXML" />
      </div>
    </div>
  </xsl:template>
</xsl:stylesheet>
