published.content = function (str) {
  if (published.strStartsWith(str, "###WV")) return published.markup(str.slice("###WV".length));
  if (published.strStartsWith(str, "###HTML")) return str.slice("###HTML".length);
  return published.strEscapeHTML(str);
};

published.markup = (function () {
  var State = {
    NONE: 0,
    PARAGRAPH: 1,
    PARAGRAPH_NEXT: 2,
    SPAN: 3,
    SPAN_END: 4,
    CLASS: 5,
    CLASS_NEXT: 6,
    HEADING: 7,
    HEADING_BODY: 8,
    LIST: 9,
    LIST_ITEM: 10,
    LIST_NEXT: 11,
    KEY: 12,
    KEY_BODY: 13,
    VALUE: 14,
    VALUE_BODY: 15,
    KEY_NEXT: 16
  };
  var entities = {
    ' ': '&nbsp;',
    '!': '&#33;',
    '#': '&#35;',
    '$': '&#36;',
    '*': '&#42;',
    '<': '&lt;',
    '=': '&#61;',
    '>': '&gt;',
    '{': '&#123;',
    '}': '&#125;'
  };
  return function (str) {
    var out = '';
    var state = State.NONE;
    var stack = [];
    var out_stack = [];
    var level = 0;
    var type;
    var type_last;
    var list_stack = [];
    var in_kv;

    for (var i = 0, length = str.length; i < length; i++) {
      var c = str.charAt(i);
      var n = str.charAt(i + 1);
      switch (c) {
        case '\r': if (n === '\n') { i++; n = str.charAt(i + 1); }
        case '\n': c = '\n'; break;
        case '<': c = '&lt;'; break;
        case '>': c = '&gt;'; break;
        case '&':
          if (str.charAt(i + 2) !== ';') break;
          var ent = entities[n];
          if (typeof (ent) !== tUNDEFINED) c = ent;
          i += 2;
          break;
      }

      switch (state) {
        case /*State.NONE*/ 0:
        case /*State.PARAGRAPH_NEXT*/ 2:
        case /*State.LIST_NEXT*/ 11:
        case /*State.KEY_NEXT*/ 16: {
          switch (c) {
            case '\n':
              if (state === State.PARAGRAPH_NEXT) out += '</p>';
              else {
                if (state === State.LIST_NEXT) close_list(0);
                if (state === State.KEY_NEXT) close_kv(false);
              }
              state = State.NONE;
              break;
            case '{':
              if (state === State.PARAGRAPH_NEXT) out += ' ';
              else {
                if (state === State.LIST_NEXT) close_list(0);
                if (state === State.KEY_NEXT) close_kv(false);
                out += '<p>';
              }
              stack.push(State.PARAGRAPH);
              state = State.SPAN;
              out_stack.push(out);
              out = c;
              break;
            case '!':
              if (state === State.PARAGRAPH_NEXT) {
                state = State.PARAGRAPH;
                out += ' ' + c;
              } else {
                if (state === State.LIST_NEXT) close_list(0);
                if (state === State.KEY_NEXT) close_kv(false);
                state = State.HEADING;
                out_stack.push(out);
                out = c;
              }
              break;
            case '#':
            case '*':
              if (state === State.PARAGRAPH_NEXT) {
                state = State.PARAGRAPH;
                out += ' ' + c;
              } else {
                if (state === State.KEY_NEXT) close_kv(false);
                type = c;
                state = State.LIST;
                out_stack.push(out);
                out = c;
              }
              break;
            case '$':
              if (state === State.PARAGRAPH_NEXT) {
                state = State.PARAGRAPH;
                out += ' ' + c;
              } else {
                if (state === State.KEY_NEXT) close_kv(true);
                in_kv = state === State.KEY_NEXT;
                state = State.KEY;
                out_stack.push(out);
                out = c;
              }
              break;
            default:
              if (state === State.PARAGRAPH_NEXT) out += ' ';
              else {
                if (state === State.LIST_NEXT) close_list(0);
                if (state === State.KEY_NEXT) close_kv(false);
                out += '<p>';
              }
              state = State.PARAGRAPH;
              out += c;
              break;
          }
        } break;
        case /*State.PARAGRAPH*/ 1: {
          switch (c) {
            case '\n':
              state = State.PARAGRAPH_NEXT;
              break;
            case '{':
              stack.push(state);
              state = State.SPAN;
              out_stack.push(out);
              out = c;
              break;
            default:
              out += c;
              break;
          }
        } break;
        case /*State.SPAN*/ 3: {
          switch (c) {
            case '\n':
              out += ' ';
              break;
            case '{':
              stack.push(state);
              state = State.SPAN;
              out_stack.push(out);
              out = c;
              break;
            case '}':
              state = State.SPAN_END;
              out = out.substr(1);
              break;
            default: out += c; break;
          }
        } break;
        case /*State.SPAN_END*/ 4: {
          switch (c) {
            case '.':
              if (class_start(n)) {
                state = State.CLASS;
                out_stack.push(out);
                out = c;
                break;
              }
            default:
              state = stack.pop();
              out = out_stack.pop() + out;
              i--;
              break;
          }
        } break;
        case /*State.CLASS*/ 5: {
          out += c;
          if (n === '.') state = State.CLASS_NEXT;
          else if (!class_midle(n)) {
            state = stack.pop();
            out = '<span class="' + make_class(out) + '">'
              + out_stack.pop() + '</span>';
            out = out_stack.pop() + out;
          }
        } break;
        case /*State.CLASS_NEXT*/ 6: {
          switch (c) {
            case '.':
              if (class_start(n)) {
                state = State.CLASS;
                out += c;
                break;
              }
            default:
              state = stack.pop();
              out = '<span class="' + make_class(out) + '">'
                + out_stack.pop() + '</span>.';
              out = out_stack.pop() + out;
              break;
          }
        } break;
        case /*State.HEADING*/ 7: {
          if (c === '\n') {
            out = out_stack.pop() + '<p>' + out;
            state = State.PARAGRAPH;
            i--;
            break;
          }
          if (c === '!' && out.length < 6) out += c;
          else {
            level = out.length;
            out = out_stack.pop() + '<h' + level + '>';
            state = State.HEADING_BODY;
            switch (c) {
              case '{':
                stack.push(state);
                state = State.SPAN;
                out_stack.push(out);
                out = c;
                break;
              default:
                out += c;
                break;
            }
          }
        } break;
        case /*State.HEADING_BODY*/ 8: {
          switch (c) {
            case '\n':
              state = State.NONE;
              out += '</h' + level + '>';
              level = 0;
              break;
            case '{':
              stack.push(state);
              state = State.SPAN;
              out_stack.push(out);
              out = '{';
              break;
            default:
              out += c;
              break;
          }
        } break;
        case /*State.LIST*/ 9: {
          if (c === '\n') {
            out = out_stack.pop() + '<p>' + out;
            state = State.PARAGRAPH;
            i--;
            break;
          }
          if (c === type) out += c;
          else {
            state = State.LIST_ITEM;
            var new_level = out.length;
            out = out_stack.pop();
            if (level === new_level && type !== type_last)
              close_list(0);
            if (level === new_level) {
              out += '</li><li>';
            } else if (level < new_level) {
              for (; level < new_level; level++) {
                switch (type) {
                  case '#': out += '<ol><li>'; list_stack.push('ol'); break;
                  case '*': out += '<ul><li>'; list_stack.push('ul'); break;
                }
              }
            } else close_list(new_level);
            switch (c) {
              case '{':
                stack.push(state);
                state = State.SPAN;
                out_stack.push(out);
                out = c;
                break;
              default:
                out += c;
                break;
            }
            break;
          }
        } break;
        case /*State.LIST_ITEM*/ 10: {
          switch (c) {
            case '\n':
              type_last = type;
              state = State.LIST_NEXT;
              break;
            case '{':
              stack.push(state);
              state = State.SPAN;
              out_stack.push(out);
              out = '{';
              break;
            default:
              out += c;
              break;
          }
        } break;
        case /*State.KEY*/ 12: {
          if (c === '\n' || c === '=') {
            out = out_stack.pop() + (in_kv ? '</dl>' : '') + '<p>' + out;
            state = State.PARAGRAPH;
            i--;
            break;
          }
          state = State.KEY_BODY;
          switch (c) {
            case '{':
              stack.push(state);
              state = State.SPAN;
              out_stack.push(out);
              out = c;
              break;
            default:
              out += c;
              break;
          }
        } break;
        case /*State.KEY_BODY*/ 13: {
          if (c === '\n') {
            out = out_stack.pop() + (in_kv ? '</dl>' : '') + '<p>' + out;
            state = State.PARAGRAPH;
            i--;
            break;
          }
          switch (c) {
            case '{':
              stack.push(state);
              state = State.SPAN;
              out_stack.push(out);
              out = c;
              break;
            case '=':
              state = State.VALUE;
              out_stack.push(out);
              out = c;
              break;
            default:
              out += c;
              break;
          }
        } break;
        case /*State.VALUE*/ 14: {
          if (c === '\n') {
            out = '<p>' + out_stack.pop() + out;
            out = out_stack.pop() + (in_kv ? '</dl>' : '') + out;
            state = State.PARAGRAPH;
            i--;
            break;
          }
          out = '<dt>' + out_stack.pop().substr(1) + '</dt><dd>';
          out = (in_kv ? out_stack.pop() : out_stack.pop() + '<dl>') + out;
          state = State.VALUE_BODY;
          switch (c) {
            case '{':
              stack.push(state);
              state = State.SPAN;
              out_stack.push(out);
              out = c;
              break;
            default:
              out += c;
              break;
          }
        } break;
        case /*State.VALUE_BODY*/ 15: {
          switch (c) {
            case '\n':
              state = State.KEY_NEXT;
              break;
            case '{':
              stack.push(state);
              state = State.SPAN;
              out_stack.push(out);
              out = '{';
              break;
            default:
              out += c;
              break;
          }
        } break;
        default: throw 'WAVE.markup(state)';
      }
    }

    stack.push(state);

    while (typeof (state = stack.pop()) !== tUNDEFINED) {
      switch (state) {
        case /*State.NONE*/ 0: break;
        case /*State.PARAGRAPH*/ 1:
        case /*State.PARAGRAPH_NEXT*/ 2: out += '</p>'; break;
        case /*State.VALUE*/ 14: out = out_stack.pop() + out;
        case /*State.KEY*/ 12:
        case /*State.KEY_BODY*/ 13: if (in_kv) out_stack.push(out_stack.pop() + '</dl>');
        case /*State.LIST*/ 9:
        case /*State.HEADING*/ 7: out = '<p>' + out + '</p>';
        case /*State.SPAN*/ 3:
        case /*State.SPAN_END*/ 4: out = out_stack.pop() + out; break;
        case /*State.HEADING_BODY*/ 8: out += '</h' + level + '>'; break;
        case /*State.LIST_ITEM*/ 10:
        case /*State.LIST_NEXT*/ 11: close_list(0); break;
        case /*State.VALUE_BODY*/ 15:
        case /*State.KEY_NEXT*/ 16: close_kv(false); break;
        default:
          /*State.CLASS*/
          /*State.CLASS_NEXT*/
          throw 'WAVE.markup(last.state)';
      }
    }

    return out;

    function close_list(new_level) {
      var lst;
      while (level > new_level && typeof (lst = list_stack.pop()) !== tUNDEFINED) {
        out += '</li></' + lst + '>';
        level--;
      }
      if (level !== 0) out += '</li><li>';
    }
    function close_kv(in_kv) {
      out += '</dd>';
      if (!in_kv) out += '</dl>';
    }
  };

  function class_start(c) {
    return '_' === c
      || ('A' <= c && c <= 'Z')
      || ('a' <= c && c <= 'z');
  }

  function class_midle(c) {
    return '_' === c
      || '-' === c
      || ('A' <= c && c <= 'Z')
      || ('a' <= c && c <= 'z')
      || ('0' <= c && c <= '9');
  }

  function make_class(str) {
    var cls = str.substr(1).split(".");
    var uni = {};
    for (var i in cls) uni[cls[i]] = 0;
    cls = [];
    for (var k in uni) cls.push('wv-markup-' + k.toLowerCase());
    return cls.join(' ');
  }
})();