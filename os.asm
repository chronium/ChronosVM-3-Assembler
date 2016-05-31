inb $00, $01
inb $01, $01
inb $02, $00

mov SP, $10000
xor X
xor Y

jmp main

resx:
	cmp X, 80
	jl .done
	xor X
	inc Y
.done:
	ret

putc:
	mov Z, $A0400
	mov W, Y
	mul W, 160
	add Z, W
	add Z, X
	cmp B, '\n'
	je .newline
	movw (Z), (color)
	inc Z
	movw (Z), B
	add X, 2
	ret
.newline:
	xor X
	inc Y
	ret

puts:
	movw B, (A)
	cmp B, $0
	je .done
	call putc
	inc A
	jmp puts
.done:
	ret

coltest:
	mov (color), $00
	mov E, 0
.loop:
	cmp E, $0F
je .done
	mov B, 'A'
	call putc
	inc B
	movw F, (color)
	inc F
	movw (color), F
	inc E
jmp .loop
.done:
	mov (color), $07
	ret

main:
	cli

	mov W, idtable
	add W, 68
	mov (W), int17
	ldidt idtable

	mov A, paltest
	call puts
	call coltest
	mov B, '\n'
	call putc

	mov A, msg
	call puts
	mov A, ask
	call puts

	mov B, '>'
	call putc
	sti
hang:
	hlt
	jmp hang

int17:
	cli
	outb E, $0A
	cmp E, 1
	calle press
	cmp E, 0
	calle release
	sti
	iret

press:
	outb E, $0A
	cmp E, $E1
	je .shift
	cmp E, $E5
	je .shift

	cmp (shift), 1
	je .ls
	jmp .ln
.ls:
	mov B, slookup
	jmp .cont
.ln:
	mov B, lookup
	jmp .cont
.cont:
	add B, E
	movw B, (B)
	call putc
	jmp .done
.shift:
	movw (shift), 1
.done:
	ret

release:
	outb E, $0A
	cmp E, $E1
	je .shift
	cmp E, $E5
	je .shift
	jmp .done
.shift:
	movw (shift), 0
.done:
	ret

idtable: times 72 db $0
color: db $07
paltest: db "Palette test: ", $0
msg: db "Hello World!\nNewline Works :D\n", $0
ask: db "Enter some text to have it echoed back:\n", $0
shift: db $0
input: times 64 db $0
done: db "\ndone.", $0
lookup: 
	times 4 db $0
	db "abcdefghijklmnopqrstuvwxyz"
	db "1234567890"
	db "\n \b\t "
	db "-=[]\\#;'`,./ "
	db "            "
	db "              "
	db "/*-+\n"
	db "1234567890.\\"
	db "  =            "
	db "            "
	db "     ,=                  "
	db "     \n            "
	db "()[]\t\bABCDEF^*%<>&&|:# "
	db "     +-*/+      "
	db "        "
slookup: 
	times 4 db $0
	db "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
	db "!@#$%^&*()"
	db "\n \b\t "
	db "_+{}|#:\"~<>? "
	db "            "
	db "              "
	db "/*-+\n"
	db "1234567890.\\"
	db "  =            "
	db "            "
	db "     ,=                  "
	db "     \n            "
	db "()[]\t\bABCDEF^*%<>&&|:# "
	db "     +-*/+      "
	db "        "