tinymce.init({
    /* replace textarea having class .tinymce with tinymce editor */
    //selector: "textarea.tinymce",

    menubar: false,
    statusbar: false,

    branding: false,
    contextmenu: "spellchecker",
    resize: false,

    /* theme of the editor */
    theme: "silver",
    skin: "oxide",

    /* width and height of the editor */
    width: "100%",
    height: "60vh",

    /* display statusbar */
    statubar: true,

    /* plugin */
    plugins: [
        "advlist image lists charmap preview hr anchor pagebreak",
        "searchreplace visualblocks visualchars insertdatetime nonbreaking",
        "table contextmenu directionality emoticons template paste textcolor"
    ],

    /* toolbar */
    toolbar: "undo redo | styleselect | bold italic | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | link image | preview media | forecolor backcolor emoticons | table",

    //removed_menuitems: "newdocument preview undo redo cut copy paste pastetext selectall searchreplace visualaid visualblocks visualchars bold italic underline strikethrough superscript subscript formats removeformat table row column cell code",
    /* style */
    style_formats: [
        {
            title: "Headers", items: [
                { title: "Header 1", format: "h1" },
                { title: "Header 2", format: "h2" },
                { title: "Header 3", format: "h3" },
                { title: "Header 4", format: "h4" },
                { title: "Header 5", format: "h5" },
                { title: "Header 6", format: "h6" }
            ]
        },
        {
            title: "Inline", items: [
                { title: "Bold", icon: "bold", format: "bold" },
                { title: "Italic", icon: "italic", format: "italic" },
                { title: "Underline", icon: "underline", format: "underline" },
                { title: "Strikethrough", icon: "strikethrough", format: "strikethrough" },
                { title: "Superscript", icon: "superscript", format: "superscript" },
                { title: "Subscript", icon: "subscript", format: "subscript" },
                { title: "Code", icon: "code", format: "code" }
            ]
        },
        {
            title: "Blocks", items: [
                { title: "Paragraph", format: "p" },
                { title: "Blockquote", format: "blockquote" },
                { title: "Div", format: "div" },
                { title: "Pre", format: "pre" }
            ]
        },
        {
            title: "Alignment", items: [
                { title: "Left", icon: "alignleft", format: "alignleft" },
                { title: "Center", icon: "aligncenter", format: "aligncenter" },
                { title: "Right", icon: "alignright", format: "alignright" },
                { title: "Justify", icon: "alignjustify", format: "alignjustify" }
            ]
        }
    ]
});