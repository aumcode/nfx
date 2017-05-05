    var fTreeNodeIDSeed = 0;

    var TREE_NODE_TEMPLATE =  "<div id='exp_@id@' class='@wvTreeNodeButton@'></div>" +
                              "<div id='content_@id@' class='@wvTreeNodeContent@'>" +
                              "  <div id='own_@id@' class='@wvTreeNodeOwn@'></div>" +
                              "  <div id='children_@id@' class='@wvTreeNodeChildren@'>" +
                              "  </div>" +
                              "</div>";

    published.Tree = function(init) {

                var Node = function(nodeInit) {
                  nodeInit = nodeInit || {};

                  var node = this;

                  fTreeNodeIDSeed++;
                  var fElmID = "tvn_" + fTreeNodeIDSeed;

                  var fParent = nodeInit.parent || null;
                  if (fParent !== null && fParent.tree() !== tree) throw "Tree.Node.ctor(wrong parent tree)";
                  this.parent = function() { return fParent; };

                  var fDIV;
                  var fExpander;
                  var fDIVContent;
                  var fDIVOwn;
                  var fDIVChildren;
                  this.__divChildren = function() { return fParent !== null ? fDIVChildren : fTreeDIV; };

                  var fID = typeof(nodeInit.id) !== tUNDEFINED ? nodeInit.id.toString() : fTreeNodeIDSeed.toString();
                  this.id = function() { return fID; };

                          function updateExpansionContent() {
                            if (!fExpander) return;
                            fExpander.innerHTML = fExpanded ? fExpandedContent : fCollapsedContent;
                          }

                  var fExpandedContent = typeof(nodeInit.expandedContent) === tUNDEFINED ? tree.DEFAULT_NODE_EXPANDED_CONTENT : nodeInit.expandedContent;
                  this.expandedContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpandedContent;
                    fExpandedContent = val;
                    updateExpansionContent();
                  };

                  var fCollapsedContent = typeof(nodeInit.collapsedContent) === tUNDEFINED ? tree.DEFAULT_NODE_COLLAPSED_CONTENT : nodeInit.collapsedContent;
                  this.collapsedContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fCollapsedContent;
                    fCollapsedContent = val;
                    updateExpansionContent();
                  };

                  this.path = function() {
                    if (fParent === null) return "";
                    if (fParent === fRootNode) return fID;
                    return fParent.path() + "/" + fID;
                  };

                  // returns integer nesting level from root
                  this.level = function() {
                    if (fParent === null) return -1;
                    if (fParent === fRootNode) return 0;
                    return fParent.level() + 1;
                  };

                  this.html = function(val) {
                    if (fParent === null) return null;
                    if (typeof(val) === tUNDEFINED) return fDIVOwn.innerHTML;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, oldContent: fDIVOwn.innerHTML, newContent: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_CONTENT, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fDIVOwn.innerHTML = val;

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, oldContent: fDIVOwn.innerHTML, newContent: val};
                    treeEventInvoke(published.EVT_TREE_NODE_CONTENT, evtArgsAfter);
                  };

                  var fSelectable = nodeInit.selectable !== false;
                  this.selectable = function(val) {
                    if (typeof(val) === tUNDEFINED) return fSelectable;
                    fSelectable = val;
                    if (fSelected && !val) node.selected(false);
                  };

                  var fSelected = nodeInit.selected === true;
                  this.selected = function(val, supressEvents) {
                    if (fTreeSelectionType === published.TREE_SELECTION_NONE) return false;

                    if (typeof(val) === tUNDEFINED) return fSelected;
                    if (val === fSelected) return;

                    if (fSelectable || (!fSelectable && !val)) {

                      if (supressEvents !== true) {
                        var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, value: val, abort: false};
                        treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsBefore);
                        if (evtArgsBefore.abort === true) return;
                      }

                      fSelected = val;
                      fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();

                      if (supressEvents !== true) onNodeSelectionChanged(node, val);
                    }
                  };

                  this.wvTreeNode = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIV.className;
                    fDIV.className = val;
                  };

                  this.wvTreeNodeButton = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpander.className;
                    fExpander.className = val;
                  };

                  this.wvTreeNodeContent = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIVContent.className;
                    fDIVContent.className = val;
                  };


                  var fWVTreeNodeButtonExpanded = nodeInit.wvTreeNodeButtonExpanded || fNodeTemplateClsArgs.wvTreeNodeButtonExpanded;
                  this.wvTreeNodeButtonExpanded = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeButtonExpanded || fNodeTemplateClsArgs.wvTreeNodeButtonExpanded);
                    if (val === fWVTreeNodeButtonExpanded) return;
                    fWVTreeNodeButtonExpanded = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                  };

                  var fWVTreeNodeButtonCollapsed = nodeInit.wvTreeNodeButtonCollapsed || fNodeTemplateClsArgs.wvTreeNodeButtonCollapsed;
                  this.wvTreeNodeButtonCollapsed = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeButtonCollapsed || fNodeTemplateClsArgs.wvTreeNodeButtonCollapsed);
                    if (val === fWVTreeNodeButtonCollapsed) return;
                    fWVTreeNodeButtonCollapsed = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                  };


                  var fWVTreeNodeOwn = nodeInit.wvTreeNodeOwn || fNodeTemplateClsArgs.wvTreeNodeOwn;
                  this.wvTreeNodeOwn = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeOwn || fNodeTemplateClsArgs.wvTreeNodeOwn);
                    if (val === fWVTreeNodeOwn) return;
                    fWVTreeNodeOwn = val;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();
                  };

                  var fWVTreeNodeOwnSelected = nodeInit.wvTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected;
                  this.wvTreeNodeOwnSelected = function(val) {
                    if (typeof(val) === tUNDEFINED) return (fWVTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected);
                    if (val === fWVTreeNodeOwnSelected) return;
                    fWVTreeNodeOwnSelected = val;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();
                  };

                  this.wvTreeNodeChildren = function(val) {
                    if (typeof(val) === tUNDEFINED) return fDIVChildren.className;
                    fDIVChildren.className = val;
                  };

                  var fChildrenDisplayVisible = nodeInit.childrenDisplayVisible || fNodeChildrenDisplayVisible;
                  this.childrenDisplayVisible = function(val) {
                    if (typeof(val) === tUNDEFINED) return fChildrenDisplayVisible;
                    if (val === fChildrenDisplayVisible) return;
                    fChildrenDisplayVisible = val;
                    if (fExpanded) fDIVChildren.style.display = fChildrenDisplayVisible;
                  };

                  var fData = nodeInit.data;
                  this.data = function(val) {
                    if (typeof(val) === tUNDEFINED) return fData;
                    if (fData === val) return;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, oldData: fData, newData: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_DATA, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fData = val;

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, oldData: fData, newData: val};
                    treeEventInvoke(published.EVT_TREE_NODE_DATA, evtArgsAfter);
                  };

                  var fExpanded = nodeInit.expanded === true;
                  this.expanded = function(val) {
                    if (typeof(val) === tUNDEFINED) return fExpanded;
                    if (fParent === null) return; // fake root node coudn't be expanded/collapsed
                    if (fExpanded === val) return;

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: node, value: val, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_EXPANSION, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fExpanded = val;
                    fExpander.className = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();
                    fExpander.innerHTML = val ? fExpandedContent : fCollapsedContent;
                    fDIVChildren.style.display = val ? fChildrenDisplayVisible : "none";

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
                    treeEventInvoke(published.EVT_TREE_NODE_EXPANSION, evtArgsAfter);
                  };

                  var fChildren = [];
                  this.children = function() { return WAVE.arrayShallowCopy(fChildren); };
                  this.__children = function() { return fChildren; };

                  this.tree = function() { return tree; };

                  // public {

                  this.getDescendants = function() {
                    return node.__getDescendants(0);
                  };

                  this.__getDescendants = function(level) {
                    var walker = function() {
                      var childrenWalker = WAVE.arrayWalkable(fChildren).getWalker();
                      var childWalker = null;
                      var currentType = 0; // 0 - not initialized, 1 - child, 2 - grandchild
                      var nodeNum = -1;

                      this.reset = function() {
                        childrenWalker = WAVE.arrayWalkable(fChildren).getWalker();
                        currentType = 0;
                        nodeNum = -1;
                      };

                      this.moveNext = function() {
                        if (currentType === 0) {
                          return moveChildrenWalker();
                        } else if (currentType === 1) {
                          childWalker = childrenWalker.current().__getDescendants(level+1).getWalker();
                          if (!childWalker.moveNext()) {
                            return moveChildrenWalker();
                          } else {
                            currentType = 2;
                            return true;
                          }
                        } else if (currentType === 2) {
                          if (!childWalker.moveNext()) {
                            return moveChildrenWalker();
                          } else {
                            return true;
                          }
                        }

                        return true;

                        function moveChildrenWalker() {
                          if (!childrenWalker.moveNext()) {
                            currentType = 0;
                            return false;
                          } else {
                            nodeNum++;
                            currentType = 1;
                            return true;
                          }
                        }

                      };

                      this.current = function() {
                        if (currentType === 1) return {level: level, nodeNum: nodeNum, node: childrenWalker.current()};
                        if (currentType === 2) return childWalker.current();
                      };
                    };

                    var walkable = {getWalker: function() { return new walker(); }, tree: tree};
                    WAVE.extend(walkable, WAVE.Walkable);
                    return walkable;
                  };

                  this.getChildIdx = function (id, recursive) {
                    if (!recursive)
                      return WAVE.arrayWalkable(fChildren).wFirstIdx(function (c) { return WAVE.strSame(c.id(), id); });
                    else
                      return node.getDescendants(0).wFirstIdx(function (d) { return WAVE.strSame(d.node.id(), id); });
                  };

                  this.getChild = function (id, recursive) {
                    if (!recursive)
                      return WAVE.arrayWalkable(fChildren).wFirst(function (c) { return WAVE.strSame(c.id(), id); });
                    else {
                      var descendant = node.getDescendants(0).wFirst(function (d) { return WAVE.strSame(d.node.id(), id); });
                      return descendant ? descendant.node : null;
                    }
                  };

                  this.navigate = function(path) {
                    if (typeof(path)===tUNDEFINED || path===null) return null;

                    var segments;
                    if (WAVE.isArray(path)) {
                      segments = path;
                    } else if (!WAVE.strEmpty(path)) {
                      segments = path.split(/[\\/]+/);
                    } else {
                      return null;
                    }

                    function getNodeIdSegmentEqualsFunc(segment) {
                        var f = function (n) { return n.id() === segment; };
                        return f;
                    }

                    var node = null;
                    var childrenWalkable = WAVE.arrayWalkable(fChildren);
                    for(var i in segments) {
                      var segment = segments[i];
                      node = childrenWalkable.wFirst(getNodeIdSegmentEqualsFunc(segment));
                      if (node===null) return null;
                      childrenWalkable = WAVE.arrayWalkable(node.__children());
                    }
                    return node;
                  };

                  // Expands all parent nodes
                  this.unveil = function() {
                    var parent = fParent;
                    if (parent === null) return;

                    while(parent !== null) {
                      var parentParent = parent.parent();
                      if (parentParent !== null) parent.expanded(true);
                      parent = parentParent;
                    }
                  };

                  this.addChild = function(childNodeInit) {
                    if (typeof(childNodeInit) === tUNDEFINED) childNodeInit = {};

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, parentNode: node, childNodeInit: childNodeInit, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_ADD, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    childNodeInit.parent = node;
                    var childNode = new Node(childNodeInit);
                    fChildren.push(childNode);
                    if (fParent !== null)
                      fExpander.style.visibility = "visible";

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, parentNode: node, childNode: childNode};
                    treeEventInvoke(published.EVT_TREE_NODE_ADD, evtArgsAfter);

                    return childNode;
                  };

                  this.__removeChild = function(childNode) {
                    WAVE.arrayDelete(fChildren, childNode);
                    if (fChildren.length === 0 && fParent !== null)
                      fExpander.style.visibility = "hidden";
                  };

                  this.remove = function() {
                    if (fParent === null) {
                      console.error("Root node couldn't be removed");
                      return;
                    }

                    var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, parentNode: fParent, childNode: node, abort: false};
                    treeEventInvoke(published.EVT_TREE_NODE_REMOVE, evtArgsBefore);
                    if (evtArgsBefore.abort === true) return;

                    fDIV.parentNode.removeChild(fDIV);
                    fParent.__removeChild(node);

                    var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, parentNode: fParent, childNode: node};
                    treeEventInvoke(published.EVT_TREE_NODE_REMOVE, evtArgsAfter);
                  };

                  this.removeAllChildren = function() {
                    var children = WAVE.arrayShallowCopy(fChildren);
                    for(var i in children) {
                      var child = children[i];
                      child.remove();
                    }
                  };

                  // public }

                  if (fParent !== null) {
                    var nodeTemplateClsArgs = {
                      id: fElmID,

                      wvTreeNode            : nodeInit.wvTreeNode            || fNodeTemplateClsArgs.wvTreeNode,
                      wvTreeNodeContent     : nodeInit.wvTreeNodeContent     || fNodeTemplateClsArgs.wvTreeNodeContent,
                      wvTreeNodeOwn         : nodeInit.wvTreeNodeOwn         || fNodeTemplateClsArgs.wvTreeNodeOwn,
                      wvTreeNodeOwnSelected : nodeInit.wvTreeNodeOwnSelected || fNodeTemplateClsArgs.wvTreeNodeOwnSelected,
                      wvTreeNodeChildren    : nodeInit.wvTreeNodeChildren    || fNodeTemplateClsArgs.wvTreeNodeChildren
                    };

                    nodeTemplateClsArgs.wvTreeNodeButton = fExpanded ? node.wvTreeNodeButtonExpanded() : node.wvTreeNodeButtonCollapsed();

                    var html = WAVE.strTemplate(TREE_NODE_TEMPLATE, nodeTemplateClsArgs);

                    var parentDIV = fParent.__divChildren();

                    fDIV = document.createElement("div");
                    fDIV.id = "root_" + fElmID;
                    var cls = "";
                    fDIV.className = nodeTemplateClsArgs.wvTreeNode;

                    fDIV.innerHTML = html;

                    parentDIV.appendChild(fDIV);

                    fDIVContent = WAVE.id("content_" + fElmID);
                    fDIVOwn = WAVE.id("own_" + fElmID);
                    fDIVChildren = WAVE.id("children_" + fElmID);

                    fDIVOwn.innerHTML = nodeInit.html;
                    fDIVOwn.className = fSelected ? node.wvTreeNodeOwnSelected() : node.wvTreeNodeOwn();

                    $(fDIVOwn).click(function() {
                      node.selected(!fSelected);
                    });

                    var exp = nodeInit.expanded === true;
                    fExpander = WAVE.id("exp_" + fElmID);
                    fExpander.innerHTML = fExpanded ? fExpandedContent : fCollapsedContent;
                    fExpander.style.visibility = "hidden";

                    $(fExpander).click(function() {
                      node.expanded(!fExpanded);
                    });

                    fDIVChildren.style.display = exp ? fChildrenDisplayVisible : "none";
                  }

                }; //Node class


      if (typeof(init)===tUNDEFINED || init===null || typeof(init.DIV)===tUNDEFINED || init.DIV===null) throw "Tree.ctor(init.DIV)";

      var tree = this;
      WAVE.extend(tree, WAVE.EventManager);

      this.DEFAULT_NODE_EXPANDED_CONTENT = '-';
      this.DEFAULT_NODE_COLLAPSED_CONTENT = '+';

      var fTreeDIV = init.DIV;

      var fNodeTemplateClsArgs = {
        wvTreeNode                : init.wvTreeNode                || published.CLS_TREE_NODE,
        wvTreeNodeButtonExpanded  : init.wvTreeNodeButtonExpanded  || published.CLS_TREE_NODE_BUTTON_EXPANDED,
        wvTreeNodeButtonCollapsed : init.wvTreeNodeButtonCollapsed || published.CLS_TREE_NODE_BUTTON_COLLAPSED,
        wvTreeNodeContent         : init.wvTreeNodeContent         || published.CLS_TREE_NODE_CONTENT,
        wvTreeNodeOwn             : init.wvTreeNodeOwn             || published.CLS_TREE_NODE_OWN,
        wvTreeNodeOwnSelected     : init.wvTreeNodeOwnSelected     || published.CLS_TREE_NODE_OWN_SELECTED,
        wvTreeNodeChildren        : init.wvTreeNodeChildren        || published.CLS_TREE_NODE_CHILDREN
      };

      var fNodeChildrenDisplayVisible = init.childrenDisplayVisible || "block";

      var fTreeSelectionType = init.treeSelectionType || published.TREE_SELECTION_NONE;
      this.treeSelectionType = function(val) {
        if (typeof(fTreeSelectionType) === tUNDEFINED) return fTreeSelectionType;
        fTreeSelectionType = val;
      };

      function onNodeSelectionChanged(node, val) {
        var evtArgsAfter;

        if (fTreeSelectionType === published.TREE_SELECTION_SINGLE) {

          if (val === true) {
            fRootNode.getDescendants().wEach(function(n) {
              if (n.node !== node && n.node.selected() === true) {

                var evtArgsBefore = { phase: published.EVT_PHASE_BEFORE, node: n.node, value: n.node.selected()};
                treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsBefore);

                n.node.selected(false, true);

                var evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: n.node, value: n.node.selected()};
                treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);
              }
            });
          }

          evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
          treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);

        } else if (fTreeSelectionType === published.TREE_SELECTION_MULTI) {
          evtArgsAfter = { phase: published.EVT_PHASE_AFTER, node: node, value: val};
          treeEventInvoke(published.EVT_TREE_NODE_SELECTION, evtArgsAfter);
        }
      }

      this.selectedNodes = function(val) {
        if (typeof(val) === tUNDEFINED)
          return fRootNode.getDescendants().wWhere(function(n) { return n.node.selected() === true;});

        if (val === null) throw "val couldn't be null";

        var selectedWalkable;
        if (WAVE.isFunction(val.wAny)) {
          selectedWalkable = val;
        } else if (WAVE.isArray(val)) {
          selectedWalkable = WAVE.arrayWalkable(val);
        } else {
          throw "val type is invalid";
        }

        fRootNode.getDescendants().wEach(function(n) {
          n.node.selected(selectedWalkable.wAny(function(nodeId){ return nodeId === n.node.id();}), true);
        });
      };

      var fSupressEvents = init.supressEvents === true;
      this.supressEvents = function(val) {
        if (typeof(val) === tUNDEFINED) return fSupressEvents;
        fSupressEvents = val;
      };

      function treeEventInvoke(evt, args) {
        if (!fSupressEvents) tree.eventInvoke(evt, args);
      }

      var fRootNode = new Node();
      this.root = function() { return fRootNode; };
    };//Tree

    var fObjectInspectorEditorIDSeed = 0, fObjectInspectorEditorIDPrefix = "objinsp_edit_";
    // Visulalizes object properties hierarchy in editable form
    published.ObjectInspector = function (obj, cfg) {
      var self = this;

      WAVE.extend(this, WAVE.EventManager);

      var fTree = new published.Tree({ DIV: cfg.div });

      cfg = cfg || {};

      var fWVObjectInspectorEditor = cfg.wvObjectInspectorEditor || published.CLS_OBJECTINSPECTOR_EDITOR;
      this.wvObjectInspectorEditor = function (val) {
        if (typeof (val) === tUNDEFINED) return fWVObjectInspectorEditor;
        fWVObjectInspectorEditor = val;
      };

      var HTML_EDITOR = '<div class="@cls@">' +
                        '  <label for="@id@">@key@</label>' +
                        '  <input type="textbox" id="@id@" value="@val@" objpath="@objpath@">' +
                        '</div>';

      function onInput(e) {
          var keys = e.target.getAttribute("objpath").split(/\//);
          var objToEdit = obj;
          for (var i = 0; i < keys.length-1; i++)
              objToEdit = objToEdit[keys[i]];

          objToEdit[keys[keys.length - 1]] = e.target.value;
      }

      function build(troot, oroot, orootpath) {
        var keys = Object.keys(oroot);
        var cn;
        for (var iKey in keys) {
          var key = keys[iKey];
          var val = oroot[key];
          if (val !== null && typeof (val) !== "object") { //leaf
            var editorID = fObjectInspectorEditorIDPrefix + (fObjectInspectorEditorIDSeed++);
            var html = WAVE.strTemplate(HTML_EDITOR, {
              id: editorID, key: key, val: val,
              objpath: orootpath ? orootpath + "/" + key : key,
              cls: fWVObjectInspectorEditor
            });

            cn = troot.addChild({ html: html });

            var input = WAVE.id(editorID);
            input.objpath = cn.path();
            input.addEventListener("input", onInput);
          } else { //branch
            cn = troot.addChild({ html: key });
            build(cn, val, orootpath ? orootpath + "/" + key : key);
          }
        }
      }

      build(fTree.root(), obj, "");
    };//ObjectInspector