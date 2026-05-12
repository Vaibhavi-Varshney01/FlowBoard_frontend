import os
import re

directory = '.'

for filename in os.listdir(directory):
    if not filename.endswith('.cs'):
        continue
    
    filepath = os.path.join(directory, filename)
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # 1. using Moq; -> using NSubstitute;
    content = content.replace('using Moq;', 'using NSubstitute;\nusing NSubstitute.ExceptionExtensions;')

    # 2. new Mock<T>(...) -> Substitute.For<T>(...)
    content = re.sub(r'new Mock<([a-zA-Z0-9_<>]+)>\((.*?)\)', r'Substitute.For<\1>(\2)', content)

    # 3. Mock<T> -> T
    content = re.sub(r'Mock<([a-zA-Z0-9_<>]+)>', r'\1', content)

    # 4. .Object -> empty (e.g. _repo.Object -> _repo)
    content = re.sub(r'\.Object\b', '', content)

    # 5. Setup -> Returns
    content = re.sub(r'_([a-zA-Z0-9_]+)\.Setup\(\s*[a-zA-Z0-9_]+\s*=>\s*[a-zA-Z0-9_]+\.(.*?)\)\s*\.ReturnsAsync\((.*?)\);', r'_\1.\2.Returns(\3);', content, flags=re.DOTALL)
    content = re.sub(r'_([a-zA-Z0-9_]+)\.Setup\(\s*[a-zA-Z0-9_]+\s*=>\s*[a-zA-Z0-9_]+\.(.*?)\)\s*\.Returns\((.*?)\);', r'_\1.\2.Returns(\3);', content, flags=re.DOTALL)

    # 6. Verify -> Received
    content = re.sub(r'_([a-zA-Z0-9_]+)\.Verify\(\s*[a-zA-Z0-9_]+\s*=>\s*[a-zA-Z0-9_]+\.(.*?),\s*Times\.Once\s*\)?;', r'_\1.Received(1).\2;', content)
    content = re.sub(r'_([a-zA-Z0-9_]+)\.Verify\(\s*[a-zA-Z0-9_]+\s*=>\s*[a-zA-Z0-9_]+\.(.*?),\s*Times\.Exactly\(([0-9]+)\)\s*\)?;', r'_\1.Received(\3).\2;', content)

    # 7. It.IsAny -> Arg.Any
    content = re.sub(r'It\.IsAny<([a-zA-Z0-9_<>]+)>\(\)', r'Arg.Any<\1>()', content)

    # 8. It.Is -> Arg.Is
    content = re.sub(r'It\.Is<([a-zA-Z0-9_<>]+)>\((.*?)\)', r'Arg.Is<\1>(\2)', content)

    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
