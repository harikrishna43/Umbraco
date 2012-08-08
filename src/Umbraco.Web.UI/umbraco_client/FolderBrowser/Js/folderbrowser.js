﻿
Umbraco.Sys.registerNamespace("Umbraco.Controls");

(function ($, Base, window, document, undefined) {

    var itemMappingOptions = {
        'create': function (o) {
            var item = ko.mapping.fromJS(o.data);
            item.selected = ko.observable(false);
            item.toggleSelected = function (itm, e) {
                
                if (this.selected())
                    return;

                if (!e.ctrlKey) {
                    for (var i = 0; i < o.parent().length; i++) {
                        o.parent()[i].selected(false);
                    }
                }

                this.selected(true);
            };
            return item;
        }
    };

    Umbraco.Controls.FolderBrowser = Base.extend({
        
        // Private
        _el: null,
        _elId: null,
        _parentId: null,
        _opts: null,
        _viewModel: null,
        
        _getChildNodes: function ()
        {
            var self = this;
            
            $.getJSON(self._opts.basePath + "/FolderBrowserService/GetChildren/" + self._parentId + "/" + self._viewModel.filterTerm(), function (data) {
                if (data != undefined && data.length > 0) {
                    ko.mapping.fromJS(data, itemMappingOptions, self._viewModel.items);
                } else {
                    self._viewModel.items([]);
                }
            });
        },
        
        _getItemById: function (id)
        {
            var self = this;
            
            var results = ko.utils.arrayFilter(self._viewModel.items(), function (item) {
                return item.Id() === id;
            });

            return results.length == 1 ? results[0] : undefined;
        },
        
        _editItem: function (id) {
            var self = this;

            var item = self._getItemById(id);
            if (item === undefined)
                throw Error("No item found with the id: " + id);

            window.location.href = "editMedia.aspx?id="+ item.Id();
        },
        
        _downloadItem: function (id) {
            var self = this;

            var item = self._getItemById(id);
            if (item === undefined)
                throw Error("No item found with the id: " + id);

            window.open(item.FileUrl(), "Download");
        },
        
        _deleteItems: function (ids)
        {
            var self = this;

            var msg = ids.length + " item" + ((ids.length > 1) ? "s" : "");

            if (confirm(window.top.uiKeys['defaultdialogs_confirmdelete'] + ' the selected ' + msg + '?\n\n'))
            {
                $(window.top).trigger("nodeDeleting", []);

                $.getJSON(self._opts.basePath + "/FolderBrowserService/Delete/" + ids.join(), function (data) {
                    if (data != undefined && data.success) {
                        //raise nodeDeleted event
                        $(window.top).trigger("nodeDeleted", []);

                        //TODO: Reload current open node in tree

                        // Reload nodes
                        self._getChildNodes();
                        
                    } else {
                        throw Error("There was an error deleting the selected nodes: " + ids.join());
                    }
                });
            }
        },
        
        _initViewModel: function () 
        {
            var self = this;
            
            // Setup the viewmode;
            self._viewModel = $.extend({}, {
                parent: self,
                filterTerm: ko.observable(''),
                items: ko.observableArray([]),
                queued: ko.observableArray([])
            });
            
            self._viewModel.filtered = ko.computed(function () {
                return self._viewModel.items();
                return ko.utils.arrayFilter(this.items(), function (item) {
                    return item.Name().toLowerCase().indexOf(self._viewModel.filterTerm()) > -1 || 
                        item.Tags().toLowerCase().indexOf(self._viewModel.filterTerm()) > -1;
                });
            }, self._viewModel);

            self._viewModel.selected = ko.computed(function() {
                return ko.utils.arrayFilter(this.items(), function(item) {
                    return item.selected();
                });
            }, self._viewModel);
            
            self._viewModel.selectedIds = ko.computed(function() {
                var ids = [];
                ko.utils.arrayForEach(this.selected(), function(item) {
                    ids.push(item.Id());
                });
                return ids;
            }, self._viewModel);

            self._viewModel.filterTerm.subscribe(function (newValue) {
                self._getChildNodes();
            });
        },
        
        _initToolbar: function () 
        {
            var self = this;
            
            // Inject the upload button into the toolbar
            var button = $("<input id='fbUploadToolbarButton' type='image' src='images/editor/upload.png' titl='Upload...' onmouseover=\"this.className='editorIconOver'\" onmouseout=\"this.className='editorIcon'\" onmouseup=\"this.className='editorIconOver'\" onmousedown=\"this.className='editorIconDown'\" />");
            button.click(function (e) {
                e.preventDefault();
                $(".upload-overlay").show();
            });

            $(".tabpage:first-child .menubar td[id$='tableContainerButtons'] .sl nobr").after(button);
        },
        
        _initOverlay: function ()
        {
            var self = this;
            
            // Inject the upload overlay
            var instructions = 'draggable' in document.createElement('span')
                ? "<h1>Drag files here to upload</h1> \
                   <p>Or, click the button below to chose the items to upload</p>"
                : "<h1>Click the browse button below to chose the items to upload</h1>";

            var overlay = $("<div class='upload-overlay'>" +
                "<div class='upload-panel'>" +
                instructions +
                "<form action=\"/umbraco/webservices/MediaUploader.ashx?format=json&action=upload&parentNodeId=" + this._parentId + "\" method=\"post\" enctype=\"multipart/form-data\">" +
                "<input id='fileupload' type='file' name='file' multiple>" +
                "<input type='hidden' name='name' />" +
                "<input type='hidden' name='replaceExisting' />" +
                "</form>" +
                "<ul class='queued' data-bind='foreach: queued'><li>" +
                "<input type='text' class='label' data-bind=\"value: name, valueUpdate: 'afterkeydown', enable: progress() == 0\" />" +
                "<span class='progress'><span data-bind=\"style: { width: progress() + '%' }\"></span></span>" +
                "<a href='' data-bind='click: cancel'><img src='images/delete.png' /></a>" +
                "</li></ul>" +
                "<button class='button upload' data-bind='enable: queued().length > 0'>Upload</button>" +
                "<input type='checkbox' id='replaceExisting' />" +
                "<label for='replaceExisting'>Overwrite existing?</label>" +
                "<a href='#' class='cancel'>Cancel</a>" +
                "</div>" +
                "</div>");

            $("body").prepend(overlay);
            
            // Create uploader
            $("#fileupload").fileUploader({
                dropTarget: ".upload-overlay",
                onAdd: function (data) {

                    // Create a bindable version of the data object
                    var file = {
                        uploaderId: data.uploaderId,
                        itemId: data.itemId,
                        name: ko.observable(data.name),
                        size: data.size,
                        progress: ko.observable(data.progress),
                        cancel: function () {
                            if (this.progress() < 100)
                                $("#fileupload").fileUploader("cancelItem", this.itemId);
                            else
                                self._viewModel.queued.remove(this);
                        }
                    };

                    file.name.subscribe(function (newValue) {
                        $("#fu-item-" + file.uploaderId + "-" + file.itemId + " input[name=name]").val(newValue);
                    });

                    // Store item back in context for easy access later
                    data.context = file;

                    // Push bindable item into queue
                    self._viewModel.queued.push(file);
                },
                onDone: function (data) {
                    switch (data.status) {
                        case 'success':
                            //self._viewModel.queued.remove(data.context);
                            break;
                        case 'error':
                            self._viewModel.queued.remove(data.context);
                            break;
                        case 'canceled':
                            self._viewModel.queued.remove(data.context);
                            break;
                    }
                },
                onProgress: function (data) {
                    data.context.progress(data.progress);
                },
                onDoneAll: function () {
                    self._getChildNodes();
                }
            });

            // Hook up uploader buttons
            $(".upload-overlay .upload").click(function (e) {
                e.preventDefault();
                $("#fileupload").fileUploader("uploadAll");
            });

            $(".upload-overlay #replaceExisting").click(function() {
                $("input[name=replaceExisting]").val($(this).is(":checked"));
            });

            $(".upload-overlay .cancel").click(function (e) {
                e.preventDefault();
                $("#fileupload").fileUploader("cancelAll");
            });

            // Listen for drag events
            $(".umbFolderBrowser").live('dragenter dragover', function (e) {
                $(".upload-overlay").show();
            });

            $(".upload-overlay").live('dragleave dragexit', function (e) {
                $(this).hide();
            }).click(function () {
                $(this).hide();
            });

            $(".upload-panel").click(function (e) {
                e.stopPropagation();
            });
        },
        
        _initContextMenu: function () 
        {
            var self = this;

            // Setup context menus
            $.contextMenu({
                selector: '.umbFolderBrowser .items li',
                callback: function (key, options) {
                    var id = options.$trigger.data("id");
                    switch (key) {
                        case "edit":
                            self._editItem(id);
                            break;
                        case "download":
                            self._downloadItem(id);
                            break;
                        case "delete":
                            self._deleteItems(self._viewModel.selectedIds());
                            break;
                    }
                },
                items: {
                    "edit": { name: "Edit", icon: "edit" },
                    "download": { name: "Download", icon: "download" },
                    "separator1": "-----",
                    "delete": { name: "Delete", icon: "delete" }
                },
                animation: { show: "fadeIn", hide: "fadeOut" }
            });
        },
        
        _initItems: function ()
        {
            var self = this;

            $(".umbFolderBrowser .items").sortable({
                helper: "clone",
                opacity: 0.6 ,
                start: function (e, ui) {
                    // Add dragging class to container
                    $(".umbFolderBrowser .items").addClass("ui-sortable-dragging");
                },
                update: function (e, ui)
                {
                    // Can't sort when filtered so just return
                    if (self._viewModel.filterTerm().length > 0)
                        return;
                    
                    //var oldIndex = self._viewModel.items.indexOf(self._viewModel.tempItem());
                    var newIndex = ui.item.index();

                    $(".umbFolderBrowser .items .selected").sort(function (a,b) {
                        return parseInt($(a).data("order")) > parseInt($(b).data("order")) ? 1 : -1;
                    }).each(function(idx, itm) {
                        var id = $(itm).data("id");
                        var item = self._getItemById(id);
                        if (item !== undefined) {
                            var oldIndex = self._viewModel.items.indexOf(item);

                            // Update the index of the current item in the array
                            self._viewModel.items.splice((newIndex + idx), 0, self._viewModel.items.splice(oldIndex, 1)[0]);
                        }
                    });
                    
                },
                stop: function (e, ui) {
                    // Remove dragging class from container
                    $(".umbFolderBrowser .items").removeClass("ui-sortable-dragging");
                    
                    if (self._viewModel.filterTerm().length > 0) {
                        $(this).sortable("cancel");
                        alert("Can't sort items which have been filtered");
                    }
                    else
                    {
                        //TODO: Update on server
                    }
                }
            });
        },

        // Constructor
        constructor: function (el, opts)
        {
            var self = this;

            // Store el info
            self._el = el;
            self._elId = el.id;

            // Grab parent id from element
            self._parentId = $(el).data("parentid");

            // Merge options with default
            self._opts = $.extend({
                // Default options go here
            }, opts);

            self._initViewModel();
            self._initToolbar();
            self._initOverlay();
            self._initContextMenu();
            self._initItems();

            // Bind the viewmodel
            ko.applyBindings(self._viewModel, el);
            ko.applyBindings(self._viewModel, $(".upload-overlay").get(0));
            
            // Grab children media items
            self._getChildNodes();
        }
        
        // Public
        
    });
    
    $.fn.folderBrowser = function (o)
    {
        if ($(this).length != 1) {
            throw "Only one folder browser can exist on the page at any one time";
        }

        return $(this).each(function () {
            var folderBrowser = new Umbraco.Controls.FolderBrowser(this, o);
            $(this).data("api", folderBrowser);
        });
    };
    
    $.fn.folderBrowserApi = function ()
    {
        //ensure there's only 1
        if ($(this).length != 1) {
            throw "Requesting the API can only match one element";
        }

        //ensure thsi is a collapse panel
        if ($(this).data("api") == null) {
            throw "The matching element had not been bound to a folderBrowser";
        }

        return $(this).data("api");
    };

})(jQuery, base2.Base, window, document)