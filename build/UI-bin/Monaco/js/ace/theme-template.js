ace.define("ace/theme/celery", ["require","exports","module"], function(require, exports, module) {
    exports.isDark = true;
    exports.cssClass = "ace-celery";
    exports.cssText = `
.ace-celery .ace_gutter {
  background: {background};
  color: {darkforeground};
}

.ace-celery .ace_print-margin {
  width: 1px;
  background: {foreground};
}

.ace-celery {
  background-color: {background};
  color: {foreground};
}

.ace-celery .ace_cursor {
  color: {foreground};
}


.ace-celery .ace_scrollbar::-webkit-scrollbar {
  height: 7px;
  width: 7px;
}

.ace-celery .ace_scrollbar::-webkit-scrollbar-track
{
  background-color: {background};
  border-radius: 10px;
}

.ace-celery .ace_scrollbar::-webkit-scrollbar-thumb {
  background-color: {lightbordercolor};
  border-radius: 10px;
  margin: 5px;
}


.ace-celery .ace_marker-layer .ace_selection {
  background: {bordercolor};
}

.ace-celery.ace_multiselect .ace_selection.ace_start {
  box-shadow: 0 0 3px 0px {bordercolor};
  border-radius: 2px;
}

.ace-celery .ace_marker-layer .ace_step {
  background: {foreground};
}

.ace-celery .ace_marker-layer .ace_bracket {
  margin: -1px 0 0 -1px;
  
  border: 1px solid {lightbordercolor};
}

.ace-celery .ace_marker-layer .ace_active-line {
  background: {highlightcolor};
}

.ace-celery .ace_gutter-active-line {
  background-color: {highlightcolor};
}

.ace-celery .ace_marker-layer .ace_selected-word {
  border: 1px solid {bordercolor};
}

.ace-celery .ace_fold {
  background-color: {lightbackground};
  border-color: {lightbackground};
}

.ace-celery .ace_keyword {
  color: #F92672;
}

.ace-celery .ace_constant.ace_language {
  color: #AE81FF;
}

.ace-celery .ace_constant.ace_numeric {
  color: #AE81FF;
}

.ace-celery .ace_constant.ace_character {
  color: #AE81FF;
}

.ace-celery .ace_constant.ace_other {
  color: #AE81FF;
}

.ace-celery .ace_support.ace_function {
  color: #66D9EF;
}

.ace-celery .ace_support.ace_constant {
  color: #66D9EF;
}

.ace-celery .ace_support.ace_class {
  font-style: italic;
  color: #66D9EF;
}

.ace-celery .ace_support.ace_type {
  font-style: italic;
  color: #66D9EF;
}

.ace-celery .ace_storage {
  color: #F92672;
}

.ace-celery .ace_storage.ace_type {
  font-style: italic;
  color: #66D9EF;
}

.ace-celery .ace_invalid {
  color: #F8F8F0;
  background-color: #F92672;
}

.ace-celery .ace_invalid.ace_deprecated {
  color: #F8F8F0;
  background-color: #AE81FF;
}

.ace-celery .ace_string {
  color: #E6DB74;
}

.ace-celery .ace_comment {
  color: #75715E;
}

.ace-celery .ace_variable {
  color: #A6E22E;
}

.ace-celery .ace_variable.ace_parameter {
  font-style: italic;
  color: #FD971F;
}

.ace-celery .ace_entity.ace_other.ace_attribute-name {
  color: #A6E22E;
}

.ace-celery .ace_entity.ace_name.ace_function {
  color: #A6E22E;
}

.ace-celery .ace_entity.ace_name.ace_tag {
  color: #F92672;
}

.ace-celery .ace_indent-guide {
  background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACCAYAAACZgbYnAAAAEklEQVQImWNgYGBgYHB3d/8PAAOIAdULw8qMAAAAAElFTkSuQmCC) right repeat-y
}

.ace-celery .ace_indent-guide-active {
  background: url("data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEAAAACAQMAAACjTyRkAAAABlBMVEUAAADCwsK76u2xAAAAAXRSTlMAQObYZgAAAAxJREFUCNdjYGBoAAAAhACBGFbxzQAAAABJRU5ErkJggg==") right repeat-y;
}

.ace-celery .ace_search {
  background: {lightbackground};
  border: 1px solid {bordercolor};
}

.ace-celery .ace_button {
  color: {foreground};
}

.ace-celery .ace_button:hover {
  background-color: {highlightcolor};
}

.ace-celery .ace_search_field {
  background-color: {lightbackground};
  color: {foreground};
  border: 1px solid {bordercolor};
}

.ace-celery .ace_searchbtn {
  background-color: {lightbackground};
  border: 1px solid {bordercolor};
}

`;

    var dom = require("../lib/dom");
    dom.importCssString(exports.cssText, exports.cssClass);
});