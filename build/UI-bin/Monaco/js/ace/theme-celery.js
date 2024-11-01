ace.define("ace/theme/celery", ["require", "exports", "module"], function(require, exports, module) {
  exports.isDark = true;
  exports.cssClass = "ace-celery";
  exports.cssText = `
.ace-celery .ace_gutter {
background: #0A0A0A;  /* Gutter Hintergrund */
color: #C6CCC7;       /* Gutter Textfarbe */
}

.ace-celery .ace_print-margin {
width: 1px;
background: #0A0A0A;  /* Druckrand Hintergrund */
}

.ace-celery {
background-color: #0A0A0A; /* Haupt-Hintergrund */
color: #F8F8F2;            /* Haupt-Textfarbe */
}

.ace-celery .ace_cursor {
color: #F8F8F0; /* Cursor Farbe */
}

.ace-celery .ace_scrollbar::-webkit-scrollbar {
height: 7px;
width: 7px;
}

.ace-celery .ace_scrollbar::-webkit-scrollbar-track {
background-color: #0A0A0A; /* Scrollbar-Track Hintergrund */
border-radius: 10px;
}

.ace-celery .ace_scrollbar::-webkit-scrollbar-thumb {
background-color: #4D4D4D; /* Scrollbar-Thumb Hintergrund */
border-radius: 10px;
margin: 5px;
}

.ace-celery .ace_marker-layer .ace_selection {
background: #333333; /* Auswahl Hintergrund */
}

.ace-celery.ace_multiselect .ace_selection.ace_start {
box-shadow: 0 0 3px 0px #333333;
border-radius: 2px;
}

.ace-celery .ace_marker-layer .ace_step {
background: #1C1C1C; /* Schrittmarkierung Hintergrund */
}

.ace-celery .ace_marker-layer .ace_bracket {
margin: -1px 0 0 -1px;
border: 1px solid #4D4D4D; /* Klammermarkierung Rahmenfarbe */
}

.ace-celery .ace_marker-layer .ace_active-line {
background: #141414; /* Aktive Zeile Hintergrund */
}

.ace-celery .ace_gutter-active-line {
background-color: #141414; /* Aktive Gutter-Zeile Hintergrund */
}

.ace-celery .ace_marker-layer .ace_selected-word {
border: 1px solid #333333; /* Ausgewähltes Wort Rahmenfarbe */
}

.ace-celery .ace_fold {
background-color: #191919; /* Faltmarkierung Hintergrund */
border-color: #191919;     /* Faltmarkierung Rahmenfarbe */
}

.ace-celery .ace_keyword {
color: #FF79C6; /* Schlüsselwort Farbe */
}

.ace-celery .ace_constant.ace_language {
color: #BD93F9; /* Sprachkonstante Farbe */
}

.ace-celery .ace_constant.ace_numeric {
color: #BD93F9; /* Numerische Konstante Farbe */
}

.ace-celery .ace_constant.ace_character {
color: #BD93F9; /* Zeichenkonstante Farbe */
}

.ace-celery .ace_constant.ace_other {
color: #BD93F9; /* Andere Konstante Farbe */
}

.ace-celery .ace_support.ace_function {
color: #8BE9FD; /* Funktionsunterstützung Farbe */
}

.ace-celery .ace_support.ace_constant {
color: #8BE9FD; /* Konstante Unterstützung Farbe */
}

.ace-celery .ace_support.ace_class {
font-style: italic;
color: #8BE9FD; /* Klassenunterstützung Farbe */
}

.ace-celery .ace_support.ace_type {
font-style: italic;
color: #8BE9FD; /* Typunterstützung Farbe */
}

.ace-celery .ace_storage {
color: #FF79C6; /* Speicher Schlüsselwort Farbe */
}

.ace-celery .ace_storage.ace_type {
font-style: italic;
color: #8BE9FD; /* Speichertyp Farbe */
}

.ace-celery .ace_invalid {
color: #F8F8F0;
background-color: #FF5555; /* Ungültige Zeichen Hintergrund */
}

.ace-celery .ace_invalid.ace_deprecated {
color: #F8F8F0;
background-color: #BD93F9; /* Veraltete Zeichen Hintergrund */
}

.ace-celery .ace_string {
color: #F1FA8C; /* Zeichenfolgenfarbe */
}

.ace-celery .ace_comment {
color: #6272A4; /* Kommentar Farbe */
}

.ace-celery .ace_variable {
color: #50FA7B; /* Variablenfarbe */
}

.ace-celery .ace_variable.ace_parameter {
font-style: italic;
color: #FFB86C; /* Parameterfarbe */
}

.ace-celery .ace_entity.ace_other.ace_attribute-name {
color: #50FA7B; /* Attributname Farbe */
}

.ace-celery .ace_entity.ace_name.ace_function {
color: #50FA7B; /* Funktionsname Farbe */
}

.ace-celery .ace_entity.ace_name.ace_tag {
color: #FF79C6; /* Tagname Farbe */
}

.ace-celery .ace_indent-guide {
background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAEklEQVQImWNgYGBgYHB3d/8PAAOIAdULw8qMAAAAAElFTkSuQmCC) right repeat-y;
}

.ace-celery .ace_indent-guide-active {
background: url("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACAQMAAACjTyRkAAAABlBMVEUAAADCwsK76u2xAAAAAXRSTlMAQObYZgAAAAxJREFUCNdjYGBoAAAAhACBGFbxzQAAAABJRU5ErkJggg==") right repeat-y;
}

.ace-celery .ace_search {
background: #0A0A0A; /* Suchfeld Hintergrund */
border: 1px solid #333333;
}

.ace-celery .ace_button {
color: #F8F8F2; /* Button Textfarbe */
}

.ace-celery .ace_button:hover {
background-color: #141414; /* Button Hover Hintergrund */
}

.ace-celery .ace_search_field {
background-color: #0A0A0A; /* Suchfeld Hintergrund */
color: #F8F8F2;            /* Suchfeld Textfarbe */
border: 1px solid #333333;
}

.ace-celery .ace_searchbtn {
background-color: #0A0A0A; /* Suchbutton Hintergrund */
border: 1px solid #333333;
}
`;

  var dom = require("../lib/dom");
  dom.importCssString(exports.cssText, exports.cssClass);
});
