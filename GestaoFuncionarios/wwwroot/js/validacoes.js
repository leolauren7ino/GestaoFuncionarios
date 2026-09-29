// No projeto original esse tipo de função vinha de um bundle interno da empresa
// ("number-input-limiter"), então aqui escrevi uma versão própria, do zero, com o mesmo
// propósito: filtrar tecla por tecla (onkeydown) o que pode ser digitado num campo numérico,
// sem depender de nenhuma lib externa pra isso.
//
// Parâmetros:
//   event           - o evento de teclado (recebido automaticamente pelo onkeydown="...")
//   input           - o elemento <input> (passe sempre "this")
//   permiteDecimal  - true libera a vírgula/ponto decimal
//   permiteNegativo - true libera o sinal de "-"
//   casasDecimais   - quantos dígitos são permitidos DEPOIS da vírgula
//   tamanhoMaximo   - tamanho máximo da string inteira (incluindo vírgula/sinal)
function ForceNumericInput(event, input, permiteDecimal, permiteNegativo, casasDecimais, tamanhoMaximo) {
    const teclasDeControle = ["Backspace", "Delete", "Tab", "ArrowLeft", "ArrowRight", "ArrowUp", "ArrowDown", "Home", "End"];
    if (teclasDeControle.includes(event.key)) {
        return; // deixa passar teclas de navegação/edição sempre
    }

    const valorAtual = input.value;
    const digitando = event.key;

    if (permiteNegativo && digitando === "-" && valorAtual.indexOf("-") === -1 && input.selectionStart === 0) {
        return; // libera o "-" só no início, e só uma vez
    }

    if (permiteDecimal && (digitando === "," || digitando === ".") && valorAtual.indexOf(",") === -1) {
        return; // libera vírgula decimal, só uma vez
    }

    const ehDigito = /^[0-9]$/.test(digitando);
    if (!ehDigito) {
        event.preventDefault();
        return; // bloqueia qualquer coisa que não seja dígito (e não caiu nos casos acima)
    }

    if (valorAtual.length >= tamanhoMaximo) {
        event.preventDefault();
        return; // já bateu no tamanho máximo -> bloqueia mais dígitos
    }

    const posicaoVirgula = valorAtual.indexOf(",");
    if (permiteDecimal && posicaoVirgula !== -1) {
        const casasJaDigitadas = valorAtual.length - posicaoVirgula - 1;
        const digitandoDepoisDaVirgula = input.selectionStart > posicaoVirgula;
        if (digitandoDepoisDaVirgula && casasJaDigitadas >= casasDecimais) {
            event.preventDefault(); // já tem o número de casas decimais permitido -> bloqueia mais
        }
    }
}
