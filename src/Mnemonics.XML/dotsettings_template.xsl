<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:s="clr-namespace:System;assembly=mscorlib" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" version="1.0">

   <xsl:template match="/TemplatesExport[@family='Live Templates']">
      <wpf:ResourceDictionary xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:s="clr-namespace:System;assembly=mscorlib" xmlns:wpf="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xml:space="preserve">
         <xsl:apply-templates select="Template"/>
      </wpf:ResourceDictionary>
   </xsl:template>

   <xsl:template match="Template">
      <s:Boolean x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={@uid}/@KeyIndexDefined">True</s:Boolean>
      <s:Boolean x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={@uid}/Applicability/=Live/@EntryIndexedValue">True</s:Boolean>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={@uid}/Categories/=mnemonics/@EntryIndexedValue">mnemonics</s:String>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={@uid}/Description/@EntryValue">
         <xsl:value-of select="@description"/>
      </s:String>
      <s:Boolean x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={@uid}/Reformat/@EntryValue">
         <xsl:value-of select="@reformat"/>
      </s:Boolean>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={@uid}/Shortcut/@EntryValue">
         <xsl:value-of select="@shortcut"/>
      </s:String>
      <s:Boolean x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={@uid}/ShortenQualifiedReferences/@EntryValue">
         <xsl:value-of select="@reformat"/>
      </s:Boolean>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={@uid}/Text/@EntryValue">
         <xsl:value-of select="@text"/>
      </s:String>
      <xsl:apply-templates select="Variables/Variable">
         <xsl:with-param name="uid" select="@uid"/>
      </xsl:apply-templates>
      <xsl:apply-templates select="Context/*">
         <xsl:with-param name="uid" select="@uid"/>
      </xsl:apply-templates>
   </xsl:template>

   <xsl:template match="Variables/Variable">
      <xsl:param name="uid" select="/.."/>
      <s:Boolean x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Field/={@name}/@KeyIndexDefined">True</s:Boolean>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Field/={@name}/Expression/@EntryValue">
         <xsl:value-of select="@expression"/>
      </s:String>
      <s:Int64 x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Field/={@name}/Order/@EntryValue">
         <xsl:value-of select="@initialRange"/>
      </s:Int64>
   </xsl:template>

   <xsl:template match="Context/CSharpContext[contains(@context, 'TypeAndNamespace')]">
      <xsl:param name="uid" select="/.."/>
      <s:Boolean x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Scope/=558F05AA0DE96347816FF785232CFB2A/@KeyIndexDefined">True</s:Boolean>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Scope/=558F05AA0DE96347816FF785232CFB2A/CustomProperties/=minimumLanguageVersion/@EntryIndexedValue">
         <xsl:value-of select="@minimumLanguageVersion"/>
      </s:String>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Scope/=558F05AA0DE96347816FF785232CFB2A/Type/@EntryValue">InCSharpTypeAndNamespace</s:String>
   </xsl:template>

   <xsl:template match="Context/CSharpContext[contains(@context, 'TypeMember')]">
      <xsl:param name="uid" select="/.."/>
      <s:Boolean x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Scope/=B68999B9D6B43E47A02B22C12A54C3CC/@KeyIndexDefined">True</s:Boolean>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Scope/=B68999B9D6B43E47A02B22C12A54C3CC/CustomProperties/=minimumLanguageVersion/@EntryIndexedValue">
         <xsl:value-of select="@minimumLanguageVersion"/>
      </s:String>
      <s:String x:Key="/Default/PatternsAndTemplates/LiveTemplates/Template/={$uid}/Scope/=B68999B9D6B43E47A02B22C12A54C3CC/Type/@EntryValue">InCSharpTypeMember</s:String>
   </xsl:template>

   <xsl:template match="Context/VBContext">
      <xsl:param name="uid" select="/.."/>
      <!-- TODO -->
   </xsl:template>

</xsl:stylesheet>