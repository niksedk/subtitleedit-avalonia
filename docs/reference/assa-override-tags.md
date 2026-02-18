# ASSA Override Tags Reference

Advanced SubStation Alpha (ASS/SSA) override tags allow you to modify the appearance and position of subtitle text inline, overriding the default style settings.

## Overview

Override tags are enclosed in curly braces `{}` and can be placed anywhere in the subtitle text. They affect the text that follows them until another override tag changes the property or the line ends.

## Text Styling Tags

### Bold
- `\b1` — Enable bold
- `\b0` — Disable bold
- `\b<weight>` — Set font weight (100-900)

### Italic
- `\i1` — Enable italic
- `\i0` — Disable italic

### Underline
- `\u1` — Enable underline
- `\u0` — Disable underline

### Strikeout
- `\s1` — Enable strikeout
- `\s0` — Disable strikeout

### Border/Outline
- `\bord<width>` — Set border/outline width in pixels
- `\bord0` — Remove border/outline

### Shadow
- `\shad<depth>` — Set shadow depth in pixels
- `\shad0` — Remove shadow

### Blur
- `\blur<strength>` — Set blur strength
- `\be<strength>` — Set blur edges strength (0-100)

## Color Tags

Colors are specified in hexadecimal BGR format: `&H<BB><GG><RR>&` or `&H<AA><BB><GG><RR>&` (with alpha).

### Primary Color
- `\c&H<color>&` or `\1c&H<color>&` — Set primary (text) color

### Secondary Color
- `\2c&H<color>&` — Set secondary (karaoke) color

### Border Color
- `\3c&H<color>&` — Set border/outline color

### Shadow Color
- `\4c&H<color>&` — Set shadow color

### Alpha/Transparency
- `\1a&H<alpha>&` — Set primary alpha (00=opaque, FF=transparent)
- `\2a&H<alpha>&` — Set secondary alpha
- `\3a&H<alpha>&` — Set border alpha
- `\4a&H<alpha>&` — Set shadow alpha
- `\alpha&H<alpha>&` — Set alpha for all components

## Font Tags

### Font Name
- `\fn<font name>` — Set font name
- `\fn` — Reset to style default

### Font Size
- `\fs<size>` — Set font size in points
- `\fs` — Reset to style default

### Font Scale
- `\fscx<percent>` — Set horizontal font scale (100=normal)
- `\fscy<percent>` — Set vertical font scale (100=normal)

### Font Spacing
- `\fsp<pixels>` — Set spacing between characters in pixels

### Font Encoding
- `\fe<encoding>` — Set font encoding

## Position Tags

### Position
- `\pos(<x>,<y>)` — Set absolute position (pixels from top-left)

### Move
- `\move(<x1>,<y1>,<x2>,<y2>)` — Move from position 1 to position 2
- `\move(<x1>,<y1>,<x2>,<y2>,<t1>,<t2>)` — Move between times t1 and t2 (milliseconds)

### Alignment
- `\an<alignment>` — Set alignment (numpad style: 1=bottom-left, 2=bottom-center, 3=bottom-right, 4=middle-left, 5=center, 6=middle-right, 7=top-left, 8=top-center, 9=top-right)
- `\a<alignment>` — Legacy alignment (1=left, 2=center, 3=right, plus 5,6,7,9,10,11 for different vertical positions)

### Margins
- `\l<pixels>` — Set left margin
- `\r<pixels>` — Set right margin
- `\v<pixels>` — Set vertical margin

## Rotation Tags

- `\frx<degrees>` — Rotate around X-axis (degrees)
- `\fry<degrees>` — Rotate around Y-axis (degrees)
- `\frz<degrees>` or `\fr<degrees>` — Rotate around Z-axis (degrees)

## Animation Tags

### Fade
- `\fad(<in>,<out>)` — Simple fade in/out (milliseconds)
- `\fade(<a1>,<a2>,<a3>,<t1>,<t2>,<t3>,<t4>)` — Complex fade with alpha values and times

### Transform
- `\t(<tags>)` — Animate tags over the duration of the subtitle
- `\t(<accel>,<tags>)` — Animate with acceleration (1=linear)
- `\t(<t1>,<t2>,<tags>)` — Animate between times t1 and t2 (milliseconds)
- `\t(<t1>,<t2>,<accel>,<tags>)` — Animate between times with acceleration

### Clip
- `\clip(<x1>,<y1>,<x2>,<y2>)` — Rectangular clip
- `\clip(<scale>,<drawing>)` — Vector clip using drawing commands
- `\iclip(<x1>,<y1>,<x2>,<y2>)` — Inverse rectangular clip
- `\iclip(<scale>,<drawing>)` — Inverse vector clip

## Drawing Tags

- `\p<level>` — Set drawing mode (0=text, 1-4=drawing with different precision levels)
- Drawing commands use vector graphics: `m` (move), `l` (line), `b` (cubic Bézier curve), etc.

## Other Tags

### Reset
- `\r` — Reset to default style
- `\r<style>` — Reset to specified style

### Line Break
- `\N` — Hard line break (always breaks)
- `\n` — Soft line break (breaks if needed)

### Karaoke
- `\k<duration>` — Standard karaoke effect (1/100 second)
- `\K<duration>` or `\kf<duration>` — Fill karaoke
- `\ko<duration>` — Outline karaoke

### Wrapping Style
- `\q<style>` — Set wrapping style (0=smart, 1=end-of-line, 2=none, 3=smart with lower line wider)

## Examples

```
{\b1}Bold text{\b0} normal text
{\c&H0000FF&}Red text{\r} normal text
{\fs50}Large{\fs} normal {\fs20}small
{\pos(640,360)}Centered on screen
{\fad(500,500)}Fade in and out
{\t(\frz360)}Rotating text
```

## Multiple Tags

Multiple override tags can be combined in the same block:

```
{\b1\i1\c&HFF0000&}Bold italic blue text
{\pos(100,200)\fs40\bord3}Large text at position with thick border
```

## Notes

- Override tags are case-sensitive
- Some tags only work with certain renderers
- Complex animations and effects may impact playback performance
- Always test with your target player to ensure compatibility

## See Also

- [ASSA Styles](../features/assa-styles.md) — Managing ASS/SSA styles
- [ASSA Apply Custom Override Tags](../features/assa-override-tags.md) — Applying override tags in Subtitle Edit
- [ASSA Draw](../features/assa-draw.md) — Vector drawing tool
- [ASSA Set Position](../features/assa-set-position.md) — Positioning subtitles
